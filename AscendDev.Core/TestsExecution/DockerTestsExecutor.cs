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

                await strategy.PrepareTestFilesAsync(executionDirectory, userCode, lesson);

                var stopwatch = Stopwatch.StartNew();

                try
                {
                    var containerConfig = await strategy.CreateContainerConfigAsync(
                        containerName, executionDirectory, lesson);

                    var createContainerResponse =
                        await _dockerClient.Containers.CreateContainerAsync(containerConfig);
                    containerId = createContainerResponse.ID;

                    var started = await _dockerClient.Containers.StartContainerAsync(containerId, null);
                    if (!started)
                        throw new InvalidOperationException("Failed to start the container");

                    var executionTimeoutMs = lesson.TestConfig.TimeoutMs + 5000;
                    var executionResult = await ExecuteTestsInContainerAsync(containerId, executionTimeoutMs);

                    stopwatch.Stop();

                    result = await strategy.ProcessExecutionResultAsync(
                        executionResult.stdout,
                        executionResult.stderr,
                        executionResult.exitCode,
                        stopwatch.ElapsedMilliseconds,
                        executionDirectory,
                        lesson.TestConfig);
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