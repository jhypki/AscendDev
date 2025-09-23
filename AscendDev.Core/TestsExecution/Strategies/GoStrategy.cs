using System.Text.RegularExpressions;
using AscendDev.Core.Constants;
using AscendDev.Core.Interfaces.CodeExecution;
using AscendDev.Core.Models.CodeExecution;
using AscendDev.Core.Models.Courses;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AscendDev.Core.CodeExecution.Strategies;

public class GoStrategy(ILogger<GoStrategy> logger) : ILanguageStrategy
{
    private const string BaseImageName = DockerImages.GoTester;
    private readonly ILogger<GoStrategy> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public bool SupportsLanguage(string language)
    {
        return language.Equals(SupportedLanguages.Go, StringComparison.OrdinalIgnoreCase);
    }

    public async Task PrepareTestFilesAsync(string executionDirectory, string userCode, Lesson lesson)
    {
        _logger.LogInformation("Preparing Go test files in directory: {ExecutionDirectory}", executionDirectory);

        // Create user solution file
        await File.WriteAllTextAsync(Path.Combine(executionDirectory, "solution.go"), userCode);
        _logger.LogInformation("Created solution.go with {CodeLength} characters", userCode.Length);

        // Create test file
        var testTemplate = lesson.TestConfig.TestTemplate;
        await File.WriteAllTextAsync(Path.Combine(executionDirectory, "solution_test.go"), testTemplate);
        _logger.LogInformation("Created solution_test.go with {TemplateLength} characters", testTemplate.Length);

        // Create go.mod file
        var goModContent = @"module solution

go 1.21
";
        await File.WriteAllTextAsync(Path.Combine(executionDirectory, "go.mod"), goModContent);
        _logger.LogInformation("Created go.mod file");

        // Create a custom test config if timeout differs from default
        if (lesson.TestConfig.TimeoutMs != 5000)
        {
            var timeoutSeconds = Math.Ceiling(lesson.TestConfig.TimeoutMs / 1000.0);
            var configContent = $@"{{
  ""timeout"": {timeoutSeconds}
}}";
            await File.WriteAllTextAsync(Path.Combine(executionDirectory, "test-config.json"), configContent);
            _logger.LogInformation("Created test-config.json with {TimeoutSeconds}s timeout", timeoutSeconds);
        }

    }

    public Task<CreateContainerParameters> CreateContainerConfigAsync(string containerName, string executionDirectory,
        Lesson lesson)
    {
        var config = new CreateContainerParameters
        {
            Image = BaseImageName,
            Name = containerName,
            HostConfig = new HostConfig
            {
                Memory = lesson.TestConfig.MemoryLimitMb * 1024 * 1024,
                MemorySwap = lesson.TestConfig.MemoryLimitMb * 1024 * 1024, // Disable swap
                AutoRemove = false,
                Binds = new[] { $"{executionDirectory}:/app/test" } // Mount the test directory
            },
            WorkingDir = "/app",
            Cmd = new[] { "sh", "-c", "/app/run-tests.sh" },
            Tty = false,
            AttachStdout = true,
            AttachStderr = true,
            User = "root"
        };

        return Task.FromResult(config);
    }

    public async Task<TestResult> ProcessExecutionResultAsync(string stdout, string stderr, int exitCode,
        long executionTimeMs, string executionDirectory, TestConfig testConfig)
    {

        var result = new TestResult
        {
            Success = exitCode == 0,
            TestResults = new List<TestCaseResult>(),
            Performance = new Models.TestsExecution.PerformanceMetrics()
        };

        // Try to parse the test results from results.json file
        try
        {
            var resultsFilePath = Path.Combine(executionDirectory, "results.json");
            _logger.LogDebug("Looking for results.json at: {ResultsFilePath}", resultsFilePath);

            if (File.Exists(resultsFilePath))
            {
                _logger.LogInformation("Found results.json file at {ResultsFilePath}", resultsFilePath);

                // Read and parse the results.json file
                var jsonContent = await File.ReadAllTextAsync(resultsFilePath);

                // Log the raw content for debugging
                var rawResultsPath = Path.Combine(Path.GetTempPath(),
                    $"go-results-{DateTime.UtcNow:yyyyMMddHHmmss}.json");
                await File.WriteAllTextAsync(rawResultsPath, jsonContent);
                _logger.LogInformation("Raw test results saved to {RawResultsPath}", rawResultsPath);

                // Parse the JSON content
                var goTestResults = JsonConvert.DeserializeObject<GoTestResults>(jsonContent);

                if (goTestResults != null)
                {
                    _logger.LogInformation(
                        "Parsed Go test results: {TotalTests} tests, {PassedTests} passed, {FailedTests} failed, Duration: {Duration}s",
                        goTestResults.Summary?.Total ?? 0, goTestResults.Summary?.Passed ?? 0, goTestResults.Summary?.Failed ?? 0, goTestResults.Summary?.Duration ?? 0);

                    // Extract pure test execution time from Go test results
                    result.Performance.PureTestExecutionTimeMs = (goTestResults.Summary?.Duration ?? 0) * 1000; // Convert seconds to milliseconds

                    _logger.LogInformation("Pure test execution time: {PureExecutionTime}ms", result.Performance.PureTestExecutionTimeMs);

                    // Process each test result
                    if (goTestResults.Tests != null)
                    {
                        foreach (var testCase in goTestResults.Tests)
                        {
                            _logger.LogDebug("Processing test case: {TestName}", testCase?.Name ?? "Unknown");

                            var testCaseResult = new TestCaseResult
                            {
                                Passed = testCase?.Action == "pass",
                                TestName = testCase?.Name ?? "Unknown Test",
                                Message = testCase?.Action == "pass"
                                    ? "Test passed"
                                    : testCase?.Output ?? "Test failed"
                            };

                            result.TestResults.Add(testCaseResult);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("goTestResults.Tests is null");
                    }

                    result.Success = (goTestResults.Summary?.Failed ?? 1) == 0;
                }
                else
                {
                    _logger.LogWarning("Failed to parse results.json into GoTestResults");

                    // Add error information as a failed test
                    result.TestResults.Add(new TestCaseResult
                    {
                        Passed = false,
                        TestName = "Parser Error",
                        Message = "Failed to parse test results from results.json"
                    });

                    result.Success = false;
                }
            }
            else
            {
                _logger.LogWarning("results.json file not found at {ResultsFilePath}", resultsFilePath);

                var errorMessage = "No test results found. The tests may have failed to run.";

                if (!string.IsNullOrEmpty(stderr))
                {
                    var syntaxErrorMatch = Regex.Match(stderr, @"(syntax error:.+?)(?:\n|$)");
                    if (syntaxErrorMatch.Success)
                    {
                        errorMessage = syntaxErrorMatch.Value;
                        _logger.LogWarning("Syntax error detected: {Error}", errorMessage);
                    }
                    else
                    {
                        var compileErrorMatch = Regex.Match(stderr, @"(.*\.go:\d+:\d+:.+?)(?:\n|$)");
                        if (compileErrorMatch.Success)
                        {
                            errorMessage = compileErrorMatch.Value;
                            _logger.LogWarning("Compile error detected: {Error}", errorMessage);
                        }
                    }
                }

                result.TestResults.Add(new TestCaseResult
                {
                    Passed = false,
                    TestName = "Execution Error",
                    Message = errorMessage
                });

                result.Success = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing test results");

            result.Success = false;
            result.TestResults.Add(new TestCaseResult
            {
                Passed = false,
                TestName = "Exception",
                Message = $"Error parsing test results: {ex.Message}"
            });
        }

        // If no test results were found, generate them from test cases
        if (result.TestResults.Count == 0)
        {
            _logger.LogWarning("No test case results were found, generating test cases from lesson configuration");

            if (testConfig.TestCases?.Any() == true)
            {
                // Generate test results from test cases
                foreach (var testCase in testConfig.TestCases)
                {
                    result.TestResults.Add(new TestCaseResult
                    {
                        Passed = false,
                        TestName = !string.IsNullOrEmpty(testCase.Name)
                            ? testCase.Name
                            : !string.IsNullOrEmpty(testCase.Description)
                                ? testCase.Description
                                : "Test Case",
                        Message = "Test execution failed - no test results were produced"
                    });
                }
                result.Success = false;
            }
            else
            {
                // Fallback to default test if no test cases are configured
                result.TestResults.Add(new TestCaseResult
                {
                    Passed = false,
                    TestName = "Default Test",
                    Message = "Tests did not produce any results"
                });
                result.Success = false;
            }
        }
        else
        {
            // Fallback: If test results don't have proper names, generate them from test cases
            if (result.TestResults.Any(tr => string.IsNullOrEmpty(tr.TestName)) && testConfig.TestCases?.Any() == true)
            {
                _logger.LogInformation("Some test results are missing names, generating fallback names from test cases");
                for (int i = 0; i < Math.Min(result.TestResults.Count, testConfig.TestCases.Count); i++)
                {
                    if (string.IsNullOrEmpty(result.TestResults[i].TestName))
                    {
                        var testCase = testConfig.TestCases[i];
                        result.TestResults[i].TestName = !string.IsNullOrEmpty(testCase.Name)
                            ? testCase.Name
                            : !string.IsNullOrEmpty(testCase.Description)
                                ? testCase.Description
                                : $"Test Case {i + 1}";
                    }
                }
            }
        }

        _logger.LogInformation("Go test processing complete. Success: {Success}, TestCases: {TestCaseCount}",
            result.Success, result.TestResults.Count);

        return result;
    }

    // Classes for Go test results
    private class GoTestResults
    {
        [JsonProperty("tests")] public List<GoTestCase> Tests { get; set; } = [];
        [JsonProperty("summary")] public GoTestSummary Summary { get; set; } = new();
    }

    private class GoTestSummary
    {
        [JsonProperty("total")] public int Total { get; set; }
        [JsonProperty("passed")] public int Passed { get; set; }
        [JsonProperty("failed")] public int Failed { get; set; }
        [JsonProperty("skipped")] public int Skipped { get; set; }
        [JsonProperty("duration")] public double Duration { get; set; }
    }

    private class GoTestCase
    {
        [JsonProperty("name")] public string Name { get; set; } = string.Empty;
        [JsonProperty("action")] public string Action { get; set; } = string.Empty;
        [JsonProperty("elapsed")] public double Elapsed { get; set; }
        [JsonProperty("output")] public string? Output { get; set; }
    }
}