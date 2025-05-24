using System.Text.RegularExpressions;
using AscendDev.Core.Constants;
using AscendDev.Core.Interfaces.CodeExecution;
using AscendDev.Core.Models.CodeExecution;
using AscendDev.Core.Models.Courses;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AscendDev.Core.CodeExecution.Strategies;

public class CSharpStrategy(ILogger<CSharpStrategy> logger) : ILanguageStrategy
{
    private const string BaseImageName = DockerImages.CSharpTester;
    private readonly ILogger<CSharpStrategy> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public bool SupportsLanguage(string language)
    {
        return language.Equals(SupportedLanguages.CSharp, StringComparison.OrdinalIgnoreCase);
    }

    public async Task PrepareTestFilesAsync(string executionDirectory, string userCode, Lesson lesson)
    {
        // Create test file with user code embedded
        var testContent = lesson.TestConfig.TestTemplate.Replace("__USER_CODE__", userCode);
        await File.WriteAllTextAsync(Path.Combine(executionDirectory, "UserSolution.cs"), testContent);

        // Create a custom test runner config if timeout differs from default
        if (lesson.TestConfig.TimeoutMs != 5000)
        {
            var configContent = $@"{{
  ""timeout"": {lesson.TestConfig.TimeoutMs}
}}";
            await File.WriteAllTextAsync(Path.Combine(executionDirectory, "test-config.json"), configContent);
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
            TestResults = new List<TestCaseResult>()
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
                    $"csharp-results-{DateTime.UtcNow:yyyyMMddHHmmss}.json");
                await File.WriteAllTextAsync(rawResultsPath, jsonContent);
                _logger.LogInformation("Raw test results saved to {RawResultsPath}", rawResultsPath);

                // Parse the JSON content
                var xunitResults = JsonConvert.DeserializeObject<XUnitTestResults>(jsonContent);

                if (xunitResults != null)
                {
                    _logger.LogInformation(
                        "Parsed XUnit results: {TotalTests} tests, {PassedTests} passed, {FailedTests} failed",
                        xunitResults.Total, xunitResults.Passed, xunitResults.Failed);

                    // Process each test result
                    foreach (var testCase in xunitResults.TestCases)
                    {
                        _logger.LogDebug("Processing test case: {TestName}", testCase.Name);

                        var testCaseResult = new TestCaseResult
                        {
                            Passed = testCase.Result == "Pass",
                            TestName = testCase.Name,
                            Message = testCase.Result == "Pass"
                                ? "Test passed"
                                : testCase.ErrorMessage
                        };

                        result.TestResults.Add(testCaseResult);
                    }

                    result.Success = xunitResults.Failed == 0;
                }
                else
                {
                    _logger.LogWarning("Failed to parse results.json into XUnitTestResults");

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
                    var compilationErrorMatch = Regex.Match(stderr, "(.*?error.*?:.*)");
                    if (compilationErrorMatch.Success)
                    {
                        errorMessage = compilationErrorMatch.Value;
                        _logger.LogWarning("Compilation error detected: {Error}", errorMessage);
                    }
                }
                else
                {
                    if (stdout.Contains("Could not load file or assembly"))
                    {
                        errorMessage += " A required assembly could not be found.";
                        _logger.LogWarning("Assembly not found error detected");
                    }
                    else if (stdout.Contains("Syntax error"))
                    {
                        errorMessage += " There is a syntax error in the code.";
                        _logger.LogWarning("Syntax error detected");
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

        if (result.TestResults.Count == 0)
        {
            result.TestResults.Add(new TestCaseResult
            {
                Passed = false,
                TestName = "Default Test",
                Message = "Tests did not produce any results"
            });
            _logger.LogWarning("No test case results were found, adding default failure result");
        }

        _logger.LogInformation("C# test processing complete. Success: {Success}, TestCases: {TestCaseCount}",
            result.Success, result.TestResults.Count);

        return result;
    }

    // Classes for XUnit test results
    private class XUnitTestResults
    {
        [JsonProperty("total")] public int Total { get; set; }
        [JsonProperty("passed")] public int Passed { get; set; }
        [JsonProperty("failed")] public int Failed { get; set; }
        [JsonProperty("skipped")] public int Skipped { get; set; }
        [JsonProperty("time")] public double Time { get; set; }
        [JsonProperty("testCases")] public List<XUnitTestCase> TestCases { get; set; } = [];
    }

    private class XUnitTestCase
    {
        [JsonProperty("name")] public string Name { get; set; } = string.Empty;
        [JsonProperty("result")] public string Result { get; set; } = string.Empty;
        [JsonProperty("time")] public double Time { get; set; }
        [JsonProperty("errorMessage")] public string ErrorMessage { get; set; } = string.Empty;
        [JsonProperty("errorStackTrace")] public string ErrorStackTrace { get; set; } = string.Empty;
    }
}