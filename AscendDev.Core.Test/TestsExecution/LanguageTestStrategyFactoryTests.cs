using AscendDev.Core.Interfaces.CodeExecution;
using AscendDev.Core.TestsExecution;
using Moq;
using NUnit.Framework;

namespace AscendDev.Core.Test.TestsExecution;

[TestFixture]
public class LanguageTestStrategyFactoryTests
{
    private Mock<ILanguageTestStrategy> _csharpStrategyMock;
    private Mock<ILanguageTestStrategy> _pythonStrategyMock;
    private LanguageTestStrategyFactory _factory;

    [SetUp]
    public void Setup()
    {
        _csharpStrategyMock = new Mock<ILanguageTestStrategy>();
        _csharpStrategyMock.Setup(s => s.SupportsLanguage("csharp")).Returns(true);
        _csharpStrategyMock.Setup(s => s.SupportsLanguage(It.Is<string>(lang => lang != "csharp"))).Returns(false);

        _pythonStrategyMock = new Mock<ILanguageTestStrategy>();
        _pythonStrategyMock.Setup(s => s.SupportsLanguage("python")).Returns(true);
        _pythonStrategyMock.Setup(s => s.SupportsLanguage(It.Is<string>(lang => lang != "python"))).Returns(false);

        var strategies = new List<ILanguageTestStrategy>
        {
            _csharpStrategyMock.Object,
            _pythonStrategyMock.Object
        };

        _factory = new LanguageTestStrategyFactory(strategies);
    }

    [Test]
    public void GetStrategy_WhenLanguageIsCSharp_ReturnsCSharpStrategy()
    {
        // Act
        var strategy = _factory.GetStrategy("csharp");

        // Assert
        Assert.That(strategy, Is.SameAs(_csharpStrategyMock.Object));
        _csharpStrategyMock.Verify(s => s.SupportsLanguage("csharp"), Times.Once);
    }

    [Test]
    public void GetStrategy_WhenLanguageIsPython_ReturnsPythonStrategy()
    {
        // Act
        var strategy = _factory.GetStrategy("python");

        // Assert
        Assert.That(strategy, Is.SameAs(_pythonStrategyMock.Object));
        _pythonStrategyMock.Verify(s => s.SupportsLanguage("python"), Times.Once);
    }

    [Test]
    public void GetStrategy_WhenLanguageIsNull_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _factory.GetStrategy(null));
        Assert.That(ex.ParamName, Is.EqualTo("language"));
        Assert.That(ex.Message, Does.Contain("Language cannot be null or empty"));
    }

    [Test]
    public void GetStrategy_WhenLanguageIsEmpty_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _factory.GetStrategy(string.Empty));
        Assert.That(ex.ParamName, Is.EqualTo("language"));
        Assert.That(ex.Message, Does.Contain("Language cannot be null or empty"));
    }

    [Test]
    public void GetStrategy_WhenLanguageIsNotSupported_ThrowsNotSupportedException()
    {
        // Arrange
        var unsupportedLanguage = "unsupported";

        // Act & Assert
        var ex = Assert.Throws<NotSupportedException>(() => _factory.GetStrategy(unsupportedLanguage));
        Assert.That(ex.Message, Does.Contain($"Language '{unsupportedLanguage}' is not supported"));
    }

    [Test]
    public void Constructor_WhenStrategiesIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new LanguageTestStrategyFactory(null));
        Assert.That(ex.ParamName, Is.EqualTo("strategies"));
    }
}