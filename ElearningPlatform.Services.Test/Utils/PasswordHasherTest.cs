using ElearningPlatform.Services.Utilities;
using Microsoft.Extensions.Logging;
using Moq;

namespace ElearningPlatform.Services.Test.Utils;

public class PasswordHasherTest
{
    private Mock<ILogger<PasswordHasher>> _logger;
    private PasswordHasher _passwordHasher;


    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<PasswordHasher>>();
        _passwordHasher = new PasswordHasher(_logger.Object);
    }

    [Test]
    public void Hash_WhenCalled_ReturnsHashAndSalt()
    {
        // Arrange
        var password = "password";

        // Act
        var hash = _passwordHasher.Hash(password);

        // Assert
        Assert.IsNotNull(hash);
    }

    [Test]
    public void Hash_ShouldLogError_WhenBCryptFails()
    {
        // Arrange
        string? nullPassword = null;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => _passwordHasher.Hash(nullPassword!));

        Assert.That(ex.ParamName, Is.EqualTo("inputKey"));

        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error hashing password")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void Verify_WhenCalledWithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "password";
        var hash = _passwordHasher.Hash(password);

        // Act
        var result = _passwordHasher.Verify(password, hash);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public void Verify_WhenCalledWithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var password = "password";
        var hash = _passwordHasher.Hash(password);

        // Act
        var result = _passwordHasher.Verify("incorrectPassword", hash);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public void Verify_WhenCalledWithIncorrectHash_ReturnsFalse()
    {
        // Arrange
        var password = "password";
        _passwordHasher.Hash(password);
        var incorrectHash = "$2a$12$invalidsaltinvalidsaltinvalidsa";

        // Act
        var result = _passwordHasher.Verify(password, incorrectHash);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public void Verify_WhenCalledWithIncorrectPasswordAndHash_ReturnsFalse()
    {
        // Arrange
        var password = "password";
        _passwordHasher.Hash(password);
        var incorrectHash = "$2a$12$invalidsaltinvalidsaltinvalidsa";

        // Act
        var result = _passwordHasher.Verify("incorrectPassword", incorrectHash);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public void Verify_ShouldLogError_WhenBCryptFails()
    {
        // Arrange
        string? nullPassword = null;
        string? nullHash = null;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => _passwordHasher.Verify(nullPassword!, nullHash!));

        Assert.That(ex.ParamName, Is.EqualTo("s"));

        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error verifying password")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}