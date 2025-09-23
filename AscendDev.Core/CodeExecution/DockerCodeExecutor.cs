using System.Diagnostics;
using AscendDev.Core.Interfaces.CodeExecution;
using AscendDev.Core.Models.CodeExecution;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;

namespace AscendDev.Core.CodeExecution;

public class DockerCodeExecutor : ICodeExecutor, IDisposable
{
    private readonly DockerClient _dockerClient;
    private readonly ILogger<DockerCodeExecutor> _logger;
    private readonly ILanguageExecutionStrategyFactory _strategyFactory;
    private readonly string _workingDirectory;

    public DockerCodeExecutor(
        ILanguageExecutionStrategyFactory strategyFactory,
        ILogger<DockerCodeExecutor> logger)
    {
        _strategyFactory = strategyFactory ?? throw new ArgumentNullException(nameof(strategyFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _dockerClient = OperatingSystem.IsWindows()
            ? new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient()
            : new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();

        _workingDirectory = Path.Combine(Path.GetTempPath(), "ascenddev_code_playground");

        if (!Directory.Exists(_workingDirectory)) Directory.CreateDirectory(_workingDirectory);
    }

    public void Dispose()
    {
        _dockerClient.Dispose();
    }

    public async Task<CodeExecutionResult> ExecuteAsync(string language, string code)
    {
        var result = new CodeExecutionResult
        {
            Success = false,
            Stdout = string.Empty,
            Stderr = string.Empty,
            ExitCode = 1,
            Performance = new Models.TestsExecution.PerformanceMetrics()
        };

        var overallStopwatch = Stopwatch.StartNew();

        try
        {
            var strategy = _strategyFactory.GetStrategy(language);
            _logger.LogInformation("Using {Strategy} for language: {Language}",
                strategy.GetType().Name, language);

            var executionId = Guid.NewGuid().ToString("N");
            var containerName = $"ascenddev-playground-{executionId}";
            var executionDirectory = Path.Combine(_workingDirectory, executionId);
            string containerId = null;

            try
            {
                Directory.CreateDirectory(executionDirectory);

                if (!OperatingSystem.IsWindows())
                {
                    var chmod = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "chmod",
                            Arguments = $"777 {executionDirectory}",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false
                        }
                    };
                    chmod.Start();
                    chmod.WaitForExit();
                    if (chmod.ExitCode != 0)
                        _logger.LogWarning("Failed to set permissions on {ExecutionDirectory}", executionDirectory);
                }

                // Measure file preparation time
                var filePreparationStopwatch = Stopwatch.StartNew();
                var sourceFileName = strategy.GetSourceFileName(code);
                await File.WriteAllTextAsync(Path.Combine(executionDirectory, sourceFileName), code);
                filePreparationStopwatch.Stop();
                result.Performance.FilePreparationTimeMs = filePreparationStopwatch.ElapsedMilliseconds;

                var containerStopwatch = Stopwatch.StartNew();

                try
                {
                    var containerConfig = strategy.CreateContainerConfig(containerName, executionDirectory, language);

                    // Extract image name from container config
                    var imageName = containerConfig.Image;

                    // Ensure the image exists
                    await EnsureImageExistsAsync(imageName);

                    var createContainerResponse =
                        await _dockerClient.Containers.CreateContainerAsync(containerConfig);
                    containerId = createContainerResponse.ID;

                    // Measure container startup time
                    var containerStartupStopwatch = Stopwatch.StartNew();
                    var started = await _dockerClient.Containers.StartContainerAsync(containerId, null);
                    containerStartupStopwatch.Stop();

                    if (!started)
                        throw new InvalidOperationException("Failed to start the container");

                    result.Performance.ContainerStartupTimeMs = containerStartupStopwatch.ElapsedMilliseconds;

                    var executionTimeoutMs = 10000; // 10 seconds timeout for playground execution

                    // Measure pure execution time (container runtime)
                    var pureExecutionStopwatch = Stopwatch.StartNew();
                    var executionResult = await ExecuteCodeInContainerAsync(containerId, executionTimeoutMs);
                    pureExecutionStopwatch.Stop();

                    // For code execution, the pure execution time is the container runtime
                    result.Performance.PureTestExecutionTimeMs = pureExecutionStopwatch.ElapsedMilliseconds;

                    containerStopwatch.Stop();
                    result.Performance.ContainerExecutionTimeMs = containerStopwatch.ElapsedMilliseconds;

                    result = await strategy.ProcessExecutionResultAsync(
                        executionResult.stdout,
                        executionResult.stderr,
                        executionResult.exitCode,
                        containerStopwatch.ElapsedMilliseconds,
                        executionDirectory);

                    // Preserve the performance metrics we calculated
                    if (result.Performance == null)
                    {
                        result.Performance = new Models.TestsExecution.PerformanceMetrics();
                    }
                    result.Performance.FilePreparationTimeMs = filePreparationStopwatch.ElapsedMilliseconds;
                    result.Performance.ContainerStartupTimeMs = containerStartupStopwatch.ElapsedMilliseconds;
                    result.Performance.PureTestExecutionTimeMs = pureExecutionStopwatch.ElapsedMilliseconds;
                    result.Performance.ContainerExecutionTimeMs = containerStopwatch.ElapsedMilliseconds;
                }
                catch (TaskCanceledException)
                {
                    result.Success = false;
                    result.Stderr = "Code execution timed out. Your code may have an infinite loop or is taking too long to complete.";
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Stderr = $"Error during container execution: {ex.Message}";
                    _logger.LogError(ex, "Container execution exception");
                }

                // Measure cleanup time
                var cleanupStopwatch = Stopwatch.StartNew();
                await CleanupAsync(containerId, executionDirectory);
                cleanupStopwatch.Stop();
                result.Performance.ContainerCleanupTimeMs = cleanupStopwatch.ElapsedMilliseconds;

                // Finalize performance metrics
                overallStopwatch.Stop();
                result.Performance.TotalExecutionTimeMs = overallStopwatch.ElapsedMilliseconds;

                return result;
            }
            catch (Exception ex)
            {
                await CleanupContainerAndDirectory(containerId, containerName, executionDirectory);

                result.Success = false;
                result.Stderr = $"Error setting up execution environment: {ex.Message}";

                return result;
            }
        }
        catch (NotSupportedException ex)
        {
            _logger.LogError(ex, "Language not supported: {Language}", language);
            result.Success = false;
            result.Stderr = ex.Message;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error executing code");
            result.Success = false;
            result.Stderr = $"Unexpected error: {ex.Message}";
            return result;
        }
        finally
        {
            // Ensure performance metrics are always set
            if (result.Performance == null)
            {
                overallStopwatch.Stop();
                result.Performance = new Models.TestsExecution.PerformanceMetrics
                {
                    TotalExecutionTimeMs = overallStopwatch.ElapsedMilliseconds
                };
            }
        }
    }

    private async Task<(string stdout, string stderr, int exitCode)> ExecuteCodeInContainerAsync(
        string containerId, int timeoutMs)
    {
        try
        {
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs + 1000));

            var waitTask = _dockerClient.Containers.WaitContainerAsync(containerId, cts.Token);

            if (await Task.WhenAny(waitTask, Task.Delay(timeoutMs, cts.Token)) != waitTask)
                throw new TimeoutException($"Container execution timed out after {timeoutMs}ms");

            await waitTask;

            var (stdout, stderr, exitCode) = await GetContainerLogsAsync(containerId);

            return (stdout, stderr, (int)exitCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in ExecuteCodeInContainerAsync");

            try
            {
                var (stdout, stderr, exitCode) = await GetContainerLogsAsync(containerId);
                return (stdout, stderr, (int)exitCode);
            }
            catch (Exception logEx)
            {
                return (
                    string.Empty,
                    $"Error during code execution: {ex.Message}\nFailed to get logs: {logEx.Message}",
                    1
                );
            }
        }
    }

    private async Task EnsureImageExistsAsync(string imageName)
    {
        try
        {
            // Try to inspect the image to see if it exists locally
            await _dockerClient.Images.InspectImageAsync(imageName);
            _logger.LogInformation("Image {ImageName} exists locally", imageName);
        }
        catch (DockerImageNotFoundException)
        {
            _logger.LogInformation("Image {ImageName} not found locally, pulling from registry...", imageName);

            try
            {
                // Create progress handler for logging
                var progress = new Progress<JSONMessage>(message =>
                {
                    if (!string.IsNullOrEmpty(message.Status))
                        _logger.LogDebug("Docker pull progress: {Status} {Progress}",
                            message.Status,
                            message.Progress?.Current.ToString() ?? "");
                });

                // Pull the image
                await _dockerClient.Images.CreateImageAsync(
                    new ImagesCreateParameters
                    {
                        FromImage = imageName
                    },
                    null, // Optional auth credentials
                    progress
                );

                _logger.LogInformation("Successfully pulled image {ImageName}", imageName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to pull image {ImageName}", imageName);
                throw new InvalidOperationException($"Failed to pull Docker image {imageName}: {ex.Message}", ex);
            }
        }
    }

    private async Task<(string stdout, string stderr, long exitCode)> GetContainerLogsAsync(string containerId)
    {
        var logStream = await _dockerClient.Containers.GetContainerLogsAsync(containerId,
            false,
            new ContainerLogsParameters
            {
                Follow = true,
                ShowStdout = true,
                ShowStderr = true
            });

        var stdout = new MemoryStream();
        var stderr = new MemoryStream();

        await logStream.CopyOutputToAsync(null, stdout, stderr, CancellationToken.None);

        stdout.Seek(0, SeekOrigin.Begin);
        stderr.Seek(0, SeekOrigin.Begin);

        var stdoutResult = await new StreamReader(stdout).ReadToEndAsync();
        var stderrResult = await new StreamReader(stderr).ReadToEndAsync();

        var containerInspect = await _dockerClient.Containers.InspectContainerAsync(containerId);
        var exitCode = containerInspect.State.ExitCode;

        _logger.LogDebug("Container output - StdOut: {StdOut}, StdErr: {StdErr}, ExitCode: {ExitCode}",
            stdoutResult.Length > 500 ? stdoutResult.Substring(0, 500) + "..." : stdoutResult,
            stderrResult.Length > 500 ? stderrResult.Substring(0, 500) + "..." : stderrResult,
            exitCode);

        return (stdoutResult, stderrResult, exitCode);
    }

    private async Task CleanupAsync(string containerId, string executionDirectory)
    {
        try
        {
            if (!string.IsNullOrEmpty(containerId))
                await _dockerClient.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters
                {
                    Force = true
                });

            if (Directory.Exists(executionDirectory))
                Directory.Delete(executionDirectory, true);

            _logger.LogInformation("Cleanup completed for container {ContainerId}", containerId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during cleanup");
        }
    }

    private async Task CleanupContainerAndDirectory(string containerId, string containerName, string executionDirectory)
    {
        try
        {
            if (!string.IsNullOrEmpty(containerId))
            {
                await _dockerClient.Containers.RemoveContainerAsync(containerId,
                    new ContainerRemoveParameters { Force = true });
            }
            else if (!string.IsNullOrEmpty(containerName))
            {
                var containers = await _dockerClient.Containers.ListContainersAsync(
                    new ContainersListParameters
                    {
                        All = true,
                        Filters = new Dictionary<string, IDictionary<string, bool>>
                        {
                            { "name", new Dictionary<string, bool> { { containerName, true } } }
                        }
                    });

                foreach (var container in containers)
                    await _dockerClient.Containers.RemoveContainerAsync(container.ID,
                        new ContainerRemoveParameters { Force = true });
            }

            if (Directory.Exists(executionDirectory))
                Directory.Delete(executionDirectory, true);
        }
        catch (Exception cleanupEx)
        {
            _logger.LogWarning(cleanupEx, "Cleanup after error failed");
        }
    }
}