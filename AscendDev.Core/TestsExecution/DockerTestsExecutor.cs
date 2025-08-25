using System.Diagnostics;
using AscendDev.Core.Interfaces.CodeExecution;
using AscendDev.Core.Models.CodeExecution;
using AscendDev.Core.Models.Courses;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;

namespace AscendDev.Core.TestsExecution;

public class DockerTestsExecutor : ITestsExecutor, IDisposable
{
    private readonly DockerClient _dockerClient;
    private readonly ILogger<DockerTestsExecutor> _logger;
    private readonly ILanguageStrategyFactory _strategyFactory;
    private readonly string _workingDirectory;

    public DockerTestsExecutor(
        ILanguageStrategyFactory strategyFactory,
        ILogger<DockerTestsExecutor> logger)
    {
        _strategyFactory = strategyFactory ?? throw new ArgumentNullException(nameof(strategyFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _dockerClient = OperatingSystem.IsWindows()
            ? new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient()
            : new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();

        _workingDirectory = Path.Combine(Path.GetTempPath(), "ascenddev_code_execution");

        if (!Directory.Exists(_workingDirectory)) Directory.CreateDirectory(_workingDirectory);
    }

    public void Dispose()
    {
        _dockerClient.Dispose();
    }

    public async Task<TestResult> ExecuteAsync(string userCode, Lesson lesson)
    {
        var result = new TestResult
        {
            Success = false,
            TestResults = new List<TestCaseResult>()
        };

        var overallStopwatch = Stopwatch.StartNew();
        var performanceMetrics = new Models.TestsExecution.PerformanceMetrics();

        try
        {
            var strategy = _strategyFactory.GetStrategy(lesson.Language);
            _logger.LogInformation("Using {Strategy} for language: {Language}",
                strategy.GetType().Name, lesson.Language);

            var executionId = Guid.NewGuid().ToString("N");
            var containerName = $"ascenddev-exec-{executionId}";
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

                await strategy.PrepareTestFilesAsync(executionDirectory, userCode, lesson);

                var containerStopwatch = Stopwatch.StartNew();

                try
                {
                    var containerConfig = await strategy.CreateContainerConfigAsync(
                        containerName, executionDirectory, lesson);

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

                    performanceMetrics.ContainerStartupTimeMs = containerStartupStopwatch.ElapsedMilliseconds;

                    var executionTimeoutMs = lesson.TestConfig.TimeoutMs + 5000;
                    var executionResult = await ExecuteTestsInContainerAsync(containerId, executionTimeoutMs);

                    containerStopwatch.Stop();
                    performanceMetrics.TestFrameworkTimeMs = containerStopwatch.ElapsedMilliseconds;

                    result = await strategy.ProcessExecutionResultAsync(
                        executionResult.stdout,
                        executionResult.stderr,
                        executionResult.exitCode,
                        containerStopwatch.ElapsedMilliseconds,
                        executionDirectory,
                        lesson.TestConfig);

                    // Collect container performance metrics
                    await CollectContainerMetricsAsync(containerId, performanceMetrics);
                }
                catch (TaskCanceledException)
                {
                    result.Success = false;
                    result.TestResults.Add(new TestCaseResult
                    {
                        Passed = false,
                        TestName = "Timeout Error",
                        Message =
                            "Test execution timed out. Your code may have an infinite loop or is taking too long to complete."
                    });
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.TestResults.Add(new TestCaseResult
                    {
                        Passed = false,
                        TestName = "Execution Error",
                        Message = $"Error during container execution: {ex.Message}"
                    });
                    _logger.LogError(ex, "Container execution exception");
                }

                await CleanupAsync(containerId, executionDirectory);

                // Finalize performance metrics
                overallStopwatch.Stop();
                performanceMetrics.ExecutionTimeMs = overallStopwatch.ElapsedMilliseconds;
                performanceMetrics.TestCount = result.TestResults?.Count ?? 0;

                if (performanceMetrics.TestCount > 0 && performanceMetrics.TestFrameworkTimeMs.HasValue)
                {
                    performanceMetrics.AverageTestTimeMs = (double)performanceMetrics.TestFrameworkTimeMs.Value / performanceMetrics.TestCount;
                }

                result.Performance = performanceMetrics;
                return result;
            }
            catch (Exception ex)
            {
                await CleanupContainerAndDirectory(containerId, containerName, executionDirectory);

                result.TestResults.Add(new TestCaseResult
                {
                    Passed = false,
                    TestName = "Environment Error",
                    Message = $"Error setting up execution environment: {ex.Message}"
                });

                return result;
            }
        }
        catch (NotSupportedException ex)
        {
            _logger.LogError(ex, "Language not supported: {Language}", lesson.Language);
            result.TestResults.Add(new TestCaseResult
            {
                Passed = false,
                TestName = "Language Error",
                Message = ex.Message
            });
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error executing code");
            result.TestResults.Add(new TestCaseResult
            {
                Passed = false,
                TestName = "System Error",
                Message = $"Unexpected error: {ex.Message}"
            });
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
                    ExecutionTimeMs = overallStopwatch.ElapsedMilliseconds,
                    TestCount = result.TestResults?.Count ?? 0
                };
            }
        }
    }

    private async Task<(string stdout, string stderr, int exitCode)> ExecuteTestsInContainerAsync(
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
            _logger.LogError(ex, "Exception in ExecuteTestsInContainerAsync");

            try
            {
                var (stdout, stderr, exitCode) = await GetContainerLogsAsync(containerId);
                return (stdout, stderr, (int)exitCode);
            }
            catch (Exception logEx)
            {
                return (
                    string.Empty,
                    $"Error during test execution: {ex.Message}\nFailed to get logs: {logEx.Message}",
                    1
                );
            }
        }
    }

    // Add this method to the DockerTestsExecutor class
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

    private async Task CollectContainerMetricsAsync(string containerId, Models.TestsExecution.PerformanceMetrics performanceMetrics)
    {
        try
        {
            // Get basic container information
            var containerInspect = await _dockerClient.Containers.InspectContainerAsync(containerId);

            // Add basic container information to metrics
            if (containerInspect.State != null)
            {
                performanceMetrics.AdditionalMetrics["ContainerStatus"] = containerInspect.State.Status ?? "unknown";
                performanceMetrics.AdditionalMetrics["ContainerExitCode"] = containerInspect.State.ExitCode;

                // Parse container start/finish times if available
                if (!string.IsNullOrEmpty(containerInspect.State.StartedAt) &&
                    !string.IsNullOrEmpty(containerInspect.State.FinishedAt))
                {
                    if (DateTime.TryParse(containerInspect.State.StartedAt, out var startTime) &&
                        DateTime.TryParse(containerInspect.State.FinishedAt, out var finishTime))
                    {
                        var containerRunTime = finishTime - startTime;
                        performanceMetrics.AdditionalMetrics["ContainerRunTimeMs"] = containerRunTime.TotalMilliseconds;
                    }
                }
            }

            // Add basic resource limits from container config
            if (containerInspect.HostConfig != null)
            {
                if (containerInspect.HostConfig.Memory > 0)
                {
                    performanceMetrics.AdditionalMetrics["MemoryLimitMb"] = containerInspect.HostConfig.Memory / (1024.0 * 1024.0);
                }

                if (containerInspect.HostConfig.CPUShares > 0)
                {
                    performanceMetrics.AdditionalMetrics["CpuShares"] = containerInspect.HostConfig.CPUShares;
                }
            }

            _logger.LogDebug("Collected basic container metrics for {ContainerId}", containerId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to collect container metrics for container {ContainerId}", containerId);
            // Don't fail the entire test execution if metrics collection fails
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