using AscendDev.Core.CodeExecution.Strategies;
using AscendDev.Core.Constants;
using AscendDev.Core.Models.Courses;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AscendDev.Core.Test.TestsExecution.Strategies;

[TestFixture]
public class PythonStrategyTests
{
    private Mock<ILogger<PythonStrategy>> _loggerMock;
    private PythonStrategy _strategy;
    private string _tempDirectory;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<PythonStrategy>>();
        _strategy = new PythonStrategy(_loggerMock.Object);
        _tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Test]
    public void SupportsLanguage_WhenLanguageIsPython_ReturnsTrue()
    {
        // Act
        var result = _strategy.SupportsLanguage(SupportedLanguages.Python);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void SupportsLanguage_WhenLanguageIsNotPython_ReturnsFalse()
    {
        // Act
        var result = _strategy.SupportsLanguage("csharp");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task PrepareTestFilesAsync_CreatesTestFileWithUserCode()
    {
        // Arrange
        var userCode = "def add(a, b):\n    return a + b";
        var testTemplate = "import unittest\n__USER_CODE__\nclass TestSolution(unittest.TestCase):\n    def test_add(self):\n        pass";
        var lesson = new Lesson
        {
            TestConfig = new TestConfig
            {
                TestTemplate = testTemplate
            }
        };

        // Act
        await _strategy.PrepareTestFilesAsync(_tempDirectory, userCode, lesson);

        // Assert
        // Check solution.py file
        var solutionFilePath = Path.Combine(_tempDirectory, "solution.py");
        Assert.That(File.Exists(solutionFilePath), Is.True);
        var solutionContent = await File.ReadAllTextAsync(solutionFilePath);
        Assert.That(solutionContent, Is.EqualTo(userCode));

        // Check test_solution.py file
        var testFilePath = Path.Combine(_tempDirectory, "test_solution.py");
        Assert.That(File.Exists(testFilePath), Is.True);
        var fileContent = await File.ReadAllTextAsync(testFilePath);
        Assert.That(fileContent, Is.EqualTo(testTemplate));
    }

    [Test]
    public async Task CreateContainerConfigAsync_ReturnsValidConfiguration()
    {
        // Arrange
        var containerName = "test-container";
        var executionDirectory = "/tmp/test";
        var lesson = new Lesson
        {
            TestConfig = new TestConfig
            {
                MemoryLimitMb = 256
            }
        };

        // Act
        var config = await _strategy.CreateContainerConfigAsync(containerName, executionDirectory, lesson);

        // Assert
        Assert.That(config, Is.Not.Null);
        Assert.That(config.Image, Is.EqualTo(DockerImages.PythonTester));
        Assert.That(config.Name, Is.EqualTo(containerName));
        Assert.That(config.HostConfig.Memory, Is.EqualTo(256 * 1024 * 1024));
        Assert.That(config.HostConfig.Binds, Does.Contain($"{executionDirectory}:/app/test"));
        Assert.That(config.WorkingDir, Is.EqualTo("/app"));
        Assert.That(config.Cmd[2], Is.EqualTo("/app/run-tests.sh"));
    }

    [Test]
    public async Task ProcessExecutionResultAsync_WhenResultsFileExists_ReturnsCorrectTestResults()
    {
        // Arrange
        var stdout = "Test execution completed";
        var stderr = "";
        var exitCode = 0;
        var executionTimeMs = 1000;
        var testConfig = new TestConfig();

        // Create a mock results.json file
        var resultsJson = new
        {
            tests = new[]
            {
                new
                {
                    name = "test_add",
                    outcome = "passed",
                    message = ""
                },
                new
                {
                    name = "test_subtract",
                    outcome = "failed",
                    message = "AssertionError: -1 != 1"
                }
            },
            summary = new
            {
                total = 2,
                passed = 1,
                failed = 1,
                errors = 0,
                skipped = 0
            }
        };

        var resultsFilePath = Path.Combine(_tempDirectory, "results.json");
        await File.WriteAllTextAsync(resultsFilePath, JsonConvert.SerializeObject(resultsJson));

        // Act
        var result = await _strategy.ProcessExecutionResultAsync(stdout, stderr, exitCode, executionTimeMs, _tempDirectory, testConfig);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Success, Is.False); // One test failed
        Assert.That(result.TestResults, Has.Count.EqualTo(2));

        // Find the passed test
        var passedTest = result.TestResults.FirstOrDefault(t => t.Passed);
        Assert.That(passedTest, Is.Not.Null);
        Assert.That(passedTest.TestName, Is.EqualTo("test_add"));
        Assert.That(passedTest.Message, Is.EqualTo("Test passed"));

        // Find the failed test
        var failedTest = result.TestResults.FirstOrDefault(t => !t.Passed);
        Assert.That(failedTest, Is.Not.Null);
        Assert.That(failedTest.TestName, Is.EqualTo("test_subtract"));
        Assert.That(failedTest.Message, Is.EqualTo("AssertionError: -1 != 1"));
    }

    [Test]
    public async Task ProcessExecutionResultAsync_WhenResultsFileDoesNotExist_ReturnsErrorResult()
    {
        // Arrange
        var stdout = "Test execution failed";
        var stderr = "SyntaxError: invalid syntax";
        var exitCode = 1;
        var executionTimeMs = 1000;
        var testConfig = new TestConfig();

        // Act
        var result = await _strategy.ProcessExecutionResultAsync(stdout, stderr, exitCode, executionTimeMs, _tempDirectory, testConfig);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Success, Is.False);
        Assert.That(result.TestResults, Has.Count.EqualTo(1));
        Assert.That(result.TestResults[0].Passed, Is.False);
        Assert.That(result.TestResults[0].TestName, Is.EqualTo("Execution Error"));
        Assert.That(result.TestResults[0].Message, Does.Contain("SyntaxError: invalid syntax"));
    }

    [Test]
    public async Task ProcessExecutionResultAsync_WhenParsingResultsThrowsException_ReturnsErrorResult()
    {
        // Arrange
        var stdout = "Test execution completed";
        var stderr = "";
        var exitCode = 0;
        var executionTimeMs = 1000;
        var testConfig = new TestConfig();

        // Create an invalid JSON file
        var resultsFilePath = Path.Combine(_tempDirectory, "results.json");
        await File.WriteAllTextAsync(resultsFilePath, "{ invalid json }");

        // Act
        var result = await _strategy.ProcessExecutionResultAsync(stdout, stderr, exitCode, executionTimeMs, _tempDirectory, testConfig);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Success, Is.False);
        Assert.That(result.TestResults, Has.Count.EqualTo(1));
        Assert.That(result.TestResults[0].Passed, Is.False);
        Assert.That(result.TestResults[0].TestName, Is.EqualTo("Exception"));
        Assert.That(result.TestResults[0].Message, Does.Contain("Error parsing test results"));
    }
}