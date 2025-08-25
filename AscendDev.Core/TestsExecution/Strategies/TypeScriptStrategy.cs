using System.Text.RegularExpressions;
using AscendDev.Core.Constants;
using AscendDev.Core.Interfaces.CodeExecution;
using AscendDev.Core.Models.CodeExecution;
using AscendDev.Core.Models.Courses;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AscendDev.Core.CodeExecution.Strategies;

public class TypeScriptStrategy(ILogger<TypeScriptStrategy> logger) : ILanguageStrategy
{
    private const string BaseImageName = DockerImages.TypeScriptTester;
    private readonly ILogger<TypeScriptStrategy> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public bool SupportsLanguage(string language)
    {
        return language.Equals(SupportedLanguages.TypeScript, StringComparison.OrdinalIgnoreCase);
    }

    public async Task PrepareTestFilesAsync(string executionDirectory, string userCode, Lesson lesson)
    {
        // Create test file with user code embedded
        var testContent = lesson.TestConfig.TestTemplate.Replace("__USER_CODE__", userCode);
        await File.WriteAllTextAsync(Path.Combine(executionDirectory, "test.spec.ts"), testContent);

        // Override jest.config.js only if timeout differs from default
        if (lesson.TestConfig.TimeoutMs != 5000)
        {
            var jestConfigContent =
                """
                module.exports = {
                  preset: 'ts-jest',
                  testEnvironment: 'node',
                  testTimeout: 
                """
                + lesson.TestConfig.TimeoutMs
                + "};";

            await File.WriteAllTextAsync(Path.Combine(executionDirectory, "jest.config.js"), jestConfigContent);
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
                    $"typescript-results-{DateTime.UtcNow:yyyyMMddHHmmss}.json");
                await File.WriteAllTextAsync(rawResultsPath, jsonContent);
                _logger.LogInformation("Raw test results saved to {RawResultsPath}", rawResultsPath);

                // Parse the JSON content
                var jestResults = JsonConvert.DeserializeObject<JestTestResults>(jsonContent);

                if (jestResults != null)
                {
                    _logger.LogInformation(
                        "Parsed Jest results: {TotalTests} tests, {PassedTests} passed, {FailedTests} failed",
                        jestResults.NumTotalTests, jestResults.NumPassedTests, jestResults.NumFailedTests);

                    // Process each test result
                    foreach (var testFile in jestResults.TestResults)
                    {
                        _logger.LogDebug("Processing test file: {TestFileName}", testFile.Name);

                        foreach (var assertionResult in testFile.AssertionResults)
                        {
                            var testCaseResult = new TestCaseResult
                            {
                                Passed = assertionResult.Status == "passed",
                                TestName = string.IsNullOrEmpty(assertionResult.FullName)
                                    ? assertionResult.Title
                                    : assertionResult.FullName,
                                Message = assertionResult.Status == "passed"
                                    ? "Test passed"
                                    : string.Join(Environment.NewLine, assertionResult.FailureMessages)
                            };

                            result.TestResults.Add(testCaseResult);
                        }
                    }

                    result.Success = jestResults.Success;
                }
                else
                {
                    _logger.LogWarning("Failed to parse results.json into JestTestResults");

                    // Dodaj informację o błędzie jako nieudany test
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
                    if (stdout.Contains("Cannot find module"))
                    {
                        errorMessage += " A required module could not be found.";
                        _logger.LogWarning("Module not found error detected");
                    }
                    else if (stdout.Contains("SyntaxError"))
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

        _logger.LogInformation("TypeScript test processing complete. Success: {Success}, TestCases: {TestCaseCount}",
            result.Success, result.TestResults.Count);

        return result;
    }

    // Classes for Jest test results
    private class JestTestResults
    {
        [JsonProperty("numFailedTestSuites")] public int NumFailedTestSuites { get; set; }
        [JsonProperty("numFailedTests")] public int NumFailedTests { get; set; }
        [JsonProperty("numPassedTestSuites")] public int NumPassedTestSuites { get; set; }
        [JsonProperty("numPassedTests")] public int NumPassedTests { get; set; }
        [JsonProperty("numPendingTestSuites")] public int NumPendingTestSuites { get; set; }
        [JsonProperty("numPendingTests")] public int NumPendingTests { get; set; }

        [JsonProperty("numRuntimeErrorTestSuites")]
        public int NumRuntimeErrorTestSuites { get; set; }

        [JsonProperty("numTodoTests")] public int NumTodoTests { get; set; }
        [JsonProperty("numTotalTestSuites")] public int NumTotalTestSuites { get; set; }
        [JsonProperty("numTotalTests")] public int NumTotalTests { get; set; }
        [JsonProperty("openHandles")] public List<object> OpenHandles { get; set; } = [];
        [JsonProperty("snapshot")] public SnapshotSummary Snapshot { get; set; } = new();
        [JsonProperty("startTime")] public long StartTime { get; set; }
        [JsonProperty("success")] public bool Success { get; set; }
        [JsonProperty("testResults")] public List<TestFileResult> TestResults { get; set; } = [];
    }

    private class SnapshotSummary
    {
        [JsonProperty("added")] public int Added { get; set; }
        [JsonProperty("didUpdate")] public bool DidUpdate { get; set; }
        [JsonProperty("failure")] public bool Failure { get; set; }
        [JsonProperty("filesAdded")] public int FilesAdded { get; set; }
        [JsonProperty("filesRemoved")] public int FilesRemoved { get; set; }
        [JsonProperty("filesRemovedList")] public List<string> FilesRemovedList { get; set; } = [];
        [JsonProperty("filesUnmatched")] public int FilesUnmatched { get; set; }
        [JsonProperty("filesUpdated")] public int FilesUpdated { get; set; }
        [JsonProperty("matched")] public int Matched { get; set; }
        [JsonProperty("total")] public int Total { get; set; }
        [JsonProperty("unchecked")] public int Unchecked { get; set; }
        [JsonProperty("uncheckedKeysByFile")] public List<object> UncheckedKeysByFile { get; set; } = [];
        [JsonProperty("unmatched")] public int Unmatched { get; set; }
        [JsonProperty("updated")] public int Updated { get; set; }
    }

    private class TestFileResult
    {
        [JsonProperty("assertionResults")] public List<AssertionResult> AssertionResults { get; set; } = [];
        [JsonProperty("endTime")] public long EndTime { get; set; }
        [JsonProperty("message")] public string Message { get; set; } = string.Empty;
        [JsonProperty("name")] public string Name { get; set; } = string.Empty;
        [JsonProperty("startTime")] public long StartTime { get; set; }
        [JsonProperty("status")] public string Status { get; set; } = string.Empty;
        [JsonProperty("summary")] public string Summary { get; set; } = string.Empty;
    }

    private class AssertionResult
    {
        [JsonProperty("ancestorTitles")] public List<string> AncestorTitles { get; set; } = [];
        [JsonProperty("duration")] public int Duration { get; set; }
        [JsonProperty("failureDetails")] public List<object> FailureDetails { get; set; } = [];
        [JsonProperty("failureMessages")] public List<string> FailureMessages { get; set; } = [];
        [JsonProperty("fullName")] public string FullName { get; set; } = string.Empty;
        [JsonProperty("invocations")] public int Invocations { get; set; }
        [JsonProperty("location")] public object Location { get; set; }
        [JsonProperty("numPassingAsserts")] public int NumPassingAsserts { get; set; }
        [JsonProperty("retryReasons")] public List<object> RetryReasons { get; set; } = [];
        [JsonProperty("status")] public string Status { get; set; } = string.Empty;
        [JsonProperty("title")] public string Title { get; set; } = string.Empty;
    }
}