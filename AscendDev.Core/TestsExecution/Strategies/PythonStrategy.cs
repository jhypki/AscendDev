using System.Text.RegularExpressions;
using AscendDev.Core.Constants;
using AscendDev.Core.Interfaces.CodeExecution;
using AscendDev.Core.Models.CodeExecution;
using AscendDev.Core.Models.Courses;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AscendDev.Core.CodeExecution.Strategies;

public class PythonStrategy(ILogger<PythonStrategy> logger) : ILanguageStrategy
{
    private const string BaseImageName = DockerImages.PythonTester;
    private readonly ILogger<PythonStrategy> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public bool SupportsLanguage(string language)
    {
        return language.Equals(SupportedLanguages.Python, StringComparison.OrdinalIgnoreCase);
    }

    public async Task PrepareTestFilesAsync(string executionDirectory, string userCode, Lesson lesson)
    {
        // Create user solution file
        await File.WriteAllTextAsync(Path.Combine(executionDirectory, "solution.py"), userCode);

        // Create test file
        await File.WriteAllTextAsync(Path.Combine(executionDirectory, "test_solution.py"), lesson.TestConfig.TestTemplate);

        // Create a custom pytest config if timeout differs from default
        if (lesson.TestConfig.TimeoutMs != 5000)
        {
            var timeoutSeconds = Math.Ceiling(lesson.TestConfig.TimeoutMs / 1000.0);
            var configContent = $@"[pytest]
timeout = {timeoutSeconds}
";
            await File.WriteAllTextAsync(Path.Combine(executionDirectory, "pytest.ini"), configContent);
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
                    $"python-results-{DateTime.UtcNow:yyyyMMddHHmmss}.json");
                await File.WriteAllTextAsync(rawResultsPath, jsonContent);
                _logger.LogInformation("Raw test results saved to {RawResultsPath}", rawResultsPath);

                // Parse the JSON content
                var pytestResults = JsonConvert.DeserializeObject<PytestResults>(jsonContent);

                if (pytestResults != null)
                {
                    _logger.LogInformation(
                        "Parsed Pytest results: {TotalTests} tests, {PassedTests} passed, {FailedTests} failed, Duration: {Duration}s",
                        pytestResults.Summary.Total, pytestResults.Summary.Passed, pytestResults.Summary.Failed, pytestResults.Summary.Duration);

                    // Extract pure test execution time from pytest results
                    result.Performance.PureTestExecutionTimeMs = pytestResults.Summary.Duration * 1000; // Convert seconds to milliseconds

                    _logger.LogInformation("Pure test execution time: {PureExecutionTime}ms", result.Performance.PureTestExecutionTimeMs);

                    // Process each test result
                    foreach (var testCase in pytestResults.Tests)
                    {
                        _logger.LogDebug("Processing test case: {TestName}", testCase.Name);

                        var testCaseResult = new TestCaseResult
                        {
                            Passed = testCase.Outcome == "passed",
                            TestName = testCase.Name,
                            Message = testCase.Outcome == "passed"
                                ? "Test passed"
                                : testCase.Message ?? "Test failed"
                        };

                        result.TestResults.Add(testCaseResult);
                    }

                    result.Success = pytestResults.Summary.Failed == 0 && pytestResults.Summary.Error == 0;
                }
                else
                {
                    _logger.LogWarning("Failed to parse results.json into PytestResults");

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
                    var syntaxErrorMatch = Regex.Match(stderr, @"(SyntaxError:.+?)(?:\n|$)");
                    if (syntaxErrorMatch.Success)
                    {
                        errorMessage = syntaxErrorMatch.Value;
                        _logger.LogWarning("Syntax error detected: {Error}", errorMessage);
                    }
                    else
                    {
                        var importErrorMatch = Regex.Match(stderr, @"(ImportError:.+?)(?:\n|$)");
                        if (importErrorMatch.Success)
                        {
                            errorMessage = importErrorMatch.Value;
                            _logger.LogWarning("Import error detected: {Error}", errorMessage);
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

        _logger.LogInformation("Python test processing complete. Success: {Success}, TestCases: {TestCaseCount}",
            result.Success, result.TestResults.Count);

        return result;
    }

    // Classes for Pytest test results
    private class PytestResults
    {
        [JsonProperty("tests")] public List<PytestTestCase> Tests { get; set; } = [];
        [JsonProperty("summary")] public PytestSummary Summary { get; set; } = new();
    }

    private class PytestSummary
    {
        [JsonProperty("total")] public int Total { get; set; }
        [JsonProperty("passed")] public int Passed { get; set; }
        [JsonProperty("failed")] public int Failed { get; set; }
        [JsonProperty("skipped")] public int Skipped { get; set; }
        [JsonProperty("error")] public int Error { get; set; }
        [JsonProperty("duration")] public double Duration { get; set; }
    }

    private class PytestTestCase
    {
        [JsonProperty("name")] public string Name { get; set; } = string.Empty;
        [JsonProperty("outcome")] public string Outcome { get; set; } = string.Empty;
        [JsonProperty("duration")] public double Duration { get; set; }
        [JsonProperty("message")] public string? Message { get; set; }
        [JsonProperty("traceback")] public string? Traceback { get; set; }
    }
}