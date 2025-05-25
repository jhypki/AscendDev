using AscendDev.Core.Interfaces.CodeExecution;
using AscendDev.Core.Models.CodeExecution;
using AscendDev.Core.Models.Courses;
using AscendDev.Core.TestsExecution;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Reflection;

namespace AscendDev.Core.Test.TestsExecution;

[TestFixture]
public class DockerTestsExecutorTests
{
    private Mock<ILanguageStrategyFactory> _strategyFactoryMock;
    private Mock<ILanguageStrategy> _strategyMock;
    private Mock<ILogger<DockerTestsExecutor>> _loggerMock;
    private DockerTestsExecutor _executor;
    private string _tempWorkingDirectory;

    [SetUp]
    public void Setup()
    {
        _strategyFactoryMock = new Mock<ILanguageStrategyFactory>();
        _strategyMock = new Mock<ILanguageStrategy>();
        _loggerMock = new Mock<ILogger<DockerTestsExecutor>>();

        // Create executor with real Docker client
        _executor = new DockerTestsExecutor(_strategyFactoryMock.Object, _loggerMock.Object);

        // Create a temp working directory for tests
        _tempWorkingDirectory = Path.Combine(Path.GetTempPath(), "ascenddev_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempWorkingDirectory);

        // Replace the working directory with a temp one
        var workingDirField = typeof(DockerTestsExecutor).GetField("_workingDirectory", BindingFlags.NonPublic | BindingFlags.Instance);
        workingDirField.SetValue(_executor, _tempWorkingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        _executor.Dispose();
        if (Directory.Exists(_tempWorkingDirectory))
        {
            Directory.Delete(_tempWorkingDirectory, true);
        }
    }

    [Test]
    public void Constructor_WhenStrategyFactoryIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new DockerTestsExecutor(null, _loggerMock.Object));
        Assert.That(ex.ParamName, Is.EqualTo("strategyFactory"));
    }

    [Test]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new DockerTestsExecutor(_strategyFactoryMock.Object, null));
        Assert.That(ex.ParamName, Is.EqualTo("logger"));
    }

    [Test]
    public async Task ExecuteAsync_WhenLanguageNotSupported_ReturnsFailedResult()
    {
        // Arrange
        var userCode = "console.log('Hello, World!');";
        var lesson = new Lesson { Language = "unsupported" };

        _strategyFactoryMock.Setup(f => f.GetStrategy(lesson.Language))
            .Throws(new NotSupportedException($"Language '{lesson.Language}' is not supported"));

        // Act
        var result = await _executor.ExecuteAsync(userCode, lesson);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Success, Is.False);
        Assert.That(result.TestResults, Has.Count.EqualTo(1));
        Assert.That(result.TestResults[0].Passed, Is.False);
        Assert.That(result.TestResults[0].TestName, Is.EqualTo("Language Error"));
        Assert.That(result.TestResults[0].Message, Does.Contain("not supported"));
    }

    [Test]
    public async Task ExecuteAsync_WhenPreparationFails_ReturnsFailedResult()
    {
        // Arrange
        var userCode = "console.log('Hello, World!');";
        var lesson = new Lesson
        {
            Language = "csharp",
            TestConfig = new TestConfig()
        };

        _strategyFactoryMock.Setup(f => f.GetStrategy(lesson.Language))
            .Returns(_strategyMock.Object);

        _strategyMock.Setup(s => s.PrepareTestFilesAsync(It.IsAny<string>(), userCode, lesson))
            .ThrowsAsync(new IOException("Failed to write test files"));

        // Act
        var result = await _executor.ExecuteAsync(userCode, lesson);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Success, Is.False);
        Assert.That(result.TestResults, Has.Count.EqualTo(1));
        Assert.That(result.TestResults[0].Passed, Is.False);
        Assert.That(result.TestResults[0].TestName, Is.EqualTo("Environment Error"));
        Assert.That(result.TestResults[0].Message, Does.Contain("Failed to write test files"));
    }

    [Test]
    public async Task ExecuteAsync_WhenUnexpectedExceptionOccurs_ReturnsSystemError()
    {
        // Arrange
        var userCode = "console.log('Hello, World!');";
        var lesson = new Lesson
        {
            Language = "csharp",
            TestConfig = new TestConfig()
        };

        _strategyFactoryMock.Setup(f => f.GetStrategy(lesson.Language))
            .Throws(new InvalidOperationException("Unexpected error"));

        // Act
        var result = await _executor.ExecuteAsync(userCode, lesson);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Success, Is.False);
        Assert.That(result.TestResults, Has.Count.EqualTo(1));
        Assert.That(result.TestResults[0].Passed, Is.False);
        Assert.That(result.TestResults[0].TestName, Is.EqualTo("System Error"));
        Assert.That(result.TestResults[0].Message, Does.Contain("Unexpected error"));
    }
}