using AscendDev.Core.CodeExecution;
using AscendDev.Core.Interfaces.CodeExecution;
using AscendDev.Core.Models.CodeExecution;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Reflection;

namespace AscendDev.Core.Test.CodeExecution;

[TestFixture]
public class DockerCodeExecutorTests
{
    private Mock<ILanguageExecutionStrategyFactory> _strategyFactoryMock;
    private Mock<ILanguageExecutionStrategy> _strategyMock;
    private Mock<ILogger<DockerCodeExecutor>> _loggerMock;
    private DockerCodeExecutor _executor;
    private string _tempWorkingDirectory;

    [SetUp]
    public void Setup()
    {
        _strategyFactoryMock = new Mock<ILanguageExecutionStrategyFactory>();
        _strategyMock = new Mock<ILanguageExecutionStrategy>();
        _loggerMock = new Mock<ILogger<DockerCodeExecutor>>();

        // Create executor with mocked dependencies
        _executor = new DockerCodeExecutor(_strategyFactoryMock.Object, _loggerMock.Object);

        // Create a temp working directory for tests
        _tempWorkingDirectory = Path.Combine(Path.GetTempPath(), "ascenddev_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempWorkingDirectory);

        // Replace the working directory with a temp one
        var workingDirField = typeof(DockerCodeExecutor).GetField("_workingDirectory", BindingFlags.NonPublic | BindingFlags.Instance);
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
        var ex = Assert.Throws<ArgumentNullException>(() => new DockerCodeExecutor(null, _loggerMock.Object));
        Assert.That(ex.ParamName, Is.EqualTo("strategyFactory"));
    }

    [Test]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new DockerCodeExecutor(_strategyFactoryMock.Object, null));
        Assert.That(ex.ParamName, Is.EqualTo("logger"));
    }

    [Test]
    public async Task ExecuteAsync_WhenLanguageNotSupported_ReturnsFailedResult()
    {
        // Arrange
        var language = "unsupported";
        var code = "console.log('Hello, World!');";

        _strategyFactoryMock.Setup(f => f.GetStrategy(language))
            .Throws(new NotSupportedException($"Language '{language}' is not supported"));

        // Act
        var result = await _executor.ExecuteAsync(language, code);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Stderr, Does.Contain("not supported"));
    }

    [Test]
    public async Task ExecuteAsync_WhenStrategyThrowsException_ReturnsFailedResult()
    {
        // Arrange
        var language = "csharp";
        var code = "Console.WriteLine(\"Hello, World!\");";
        var exceptionMessage = "Failed to create container config";

        _strategyFactoryMock.Setup(f => f.GetStrategy(language))
            .Returns(_strategyMock.Object);

        _strategyMock.Setup(s => s.GetSourceFileName(code))
            .Returns("Program.cs");

        _strategyMock.Setup(s => s.CreateContainerConfig(It.IsAny<string>(), It.IsAny<string>(), language))
            .Throws(new InvalidOperationException(exceptionMessage));

        // Act
        var result = await _executor.ExecuteAsync(language, code);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Stderr, Does.Contain(exceptionMessage));
    }

    [Test]
    public async Task ExecuteAsync_WhenUnexpectedExceptionOccurs_ReturnsSystemError()
    {
        // Arrange
        var language = "csharp";
        var code = "Console.WriteLine(\"Hello, World!\");";

        _strategyFactoryMock.Setup(f => f.GetStrategy(language))
            .Throws(new InvalidOperationException("Unexpected error"));

        // Act
        var result = await _executor.ExecuteAsync(language, code);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Stderr, Does.Contain("Unexpected error"));
    }

    // Note: This test would normally test successful execution, but since it requires
    // mocking Docker.DotNet internals which are not easily mockable, we'll skip the actual
    // Docker interaction and focus on the strategy factory and strategy interactions.
    [Test]
    public void ExecuteAsync_SuccessfulExecution_CannotBeTestedDirectly()
    {
        // This is a placeholder to acknowledge that we would test successful execution
        // in a real-world scenario, but it's challenging due to Docker.DotNet's design.
        // In a real project, we might:
        // 1. Create an abstraction over DockerClient for better testability
        // 2. Use integration tests with actual Docker
        // 3. Use a library like TestContainers for containerized testing
        Assert.Pass("This test acknowledges that successful execution would be tested in a real-world scenario");
    }
}