using AscendDev.Core.CodeExecution.Strategies;
using AscendDev.Core.Constants;
using AscendDev.Core.Models.Courses;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Text;

namespace AscendDev.Core.Test.TestsExecution.Strategies;

[TestFixture]
public class TypeScriptStrategyTests
{
    private Mock<ILogger<TypeScriptStrategy>> _loggerMock;
    private TypeScriptStrategy _strategy;
    private string _tempDirectory;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<TypeScriptStrategy>>();
        _strategy = new TypeScriptStrategy(_loggerMock.Object);
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
    public void SupportsLanguage_WhenLanguageIsTypeScript_ReturnsTrue()
    {
        // Act
        var result = _strategy.SupportsLanguage(SupportedLanguages.TypeScript);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void SupportsLanguage_WhenLanguageIsNotTypeScript_ReturnsFalse()
    {
        // Act
        var result = _strategy.SupportsLanguage("python");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task PrepareTestFilesAsync_CreatesTestFileWithUserCode()
    {
        // Arrange
        var userCode = "export function add(a: number, b: number): number { return a + b; }";
        var testTemplate = "import { expect } from '@jest/globals';\n__USER_CODE__\ndescribe('add', () => { it('adds two numbers', () => { expect(add(1, 2)).toBe(3); }); });";
        var lesson = new Lesson
        {
            TestConfig = new TestConfig
            {
                TestTemplate = testTemplate,
                TimeoutMs = 5000
            }
        };

        // Act
        await _strategy.PrepareTestFilesAsync(_tempDirectory, userCode, lesson);

        // Assert
        var testFilePath = Path.Combine(_tempDirectory, "test.spec.ts");
        Assert.That(File.Exists(testFilePath), Is.True);

        var fileContent = await File.ReadAllTextAsync(testFilePath);
        var expectedContent = testTemplate.Replace("__USER_CODE__", userCode);
        Assert.That(fileContent, Is.EqualTo(expectedContent));

        // Verify no config file is created when using default timeout
        var configFilePath = Path.Combine(_tempDirectory, "jest.config.js");
        Assert.That(File.Exists(configFilePath), Is.False);
    }

    [Test]
    public async Task PrepareTestFilesAsync_CreatesConfigFileWhenTimeoutDiffersFromDefault()
    {
        // Arrange
        var userCode = "export function add(a: number, b: number): number { return a + b; }";
        var testTemplate = "__USER_CODE__";
        var customTimeout = 10000;
        var lesson = new Lesson
        {
            TestConfig = new TestConfig
            {
                TestTemplate = testTemplate,
                TimeoutMs = customTimeout
            }
        };

        // Act
        await _strategy.PrepareTestFilesAsync(_tempDirectory, userCode, lesson);

        // Assert
        var configFilePath = Path.Combine(_tempDirectory, "jest.config.js");
        Assert.That(File.Exists(configFilePath), Is.True);

        var configContent = await File.ReadAllTextAsync(configFilePath);
        Assert.That(configContent, Does.Contain($"testTimeout: {customTimeout}"));
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
                MemoryLimitMb = 512
            }
        };

        // Act
        var config = await _strategy.CreateContainerConfigAsync(containerName, executionDirectory, lesson);

        // Assert
        Assert.That(config, Is.Not.Null);
        Assert.That(config.Image, Is.EqualTo(DockerImages.TypeScriptTester));
        Assert.That(config.Name, Is.EqualTo(containerName));
        Assert.That(config.HostConfig.Memory, Is.EqualTo(512 * 1024 * 1024));
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
            numTotalTests = 2,
            numPassedTests = 1,
            numFailedTests = 1,
            success = false,
            testResults = new[]
            {
                new
                {
                    name = "test.spec.ts",
                    assertionResults = new[]
                    {
                        new
                        {
                            status = "passed",
                            title = "adds two numbers correctly",
                            fullName = "add adds two numbers correctly",
                            failureMessages = new string[] { }
                        },
                        new
                        {
                            status = "failed",
                            title = "handles negative numbers",
                            fullName = "add handles negative numbers",
                            failureMessages = new[] { "Expected: 0, Received: -2" }
                        }
                    }
                }
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

        Assert.That(result.TestResults[0].Passed, Is.True);
        Assert.That(result.TestResults[0].TestName, Is.EqualTo("add adds two numbers correctly"));
        Assert.That(result.TestResults[0].Message, Is.EqualTo("Test passed"));

        Assert.That(result.TestResults[1].Passed, Is.False);
        Assert.That(result.TestResults[1].TestName, Is.EqualTo("add handles negative numbers"));
        Assert.That(result.TestResults[1].Message, Does.Contain("Expected: 0, Received: -2"));
    }

    [Test]
    public async Task ProcessExecutionResultAsync_WhenResultsFileDoesNotExist_ReturnsErrorResult()
    {
        // Arrange
        var stdout = "Test execution failed";
        var stderr = "SyntaxError: Unexpected token";
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
        Assert.That(result.TestResults[0].Message, Does.Contain("No test results found"));
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