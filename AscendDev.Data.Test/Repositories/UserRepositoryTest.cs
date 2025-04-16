using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Auth;
using AscendDev.Data.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace AscendDev.Tests.Data.Repositories;

[TestFixture]
public class UserRepositoryTest
{
    [SetUp]
    public void Setup()
    {
        _mockSql = new Mock<ISqlExecutor>();
        _mockLogger = new Mock<ILogger<UserRepository>>();
        _userRepository = new UserRepository(_mockSql.Object, _mockLogger.Object);
    }

    private Mock<ISqlExecutor> _mockSql;
    private Mock<ILogger<UserRepository>> _mockLogger;
    private UserRepository _userRepository;

    [Test]
    public async Task GetByIdAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedUser = CreateTestUser(userId);

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<User>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userRepository.GetByIdAsync(userId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(userId));
        Assert.That(result.Email, Is.EqualTo(expectedUser.Email));
        Assert.That(result.Username, Is.EqualTo(expectedUser.Username));

        _mockSql.Verify(x => x.QueryFirstOrDefaultAsync<User>(
                It.IsAny<string>(),
                It.Is<object>(p => p.GetType().GetProperty("Id")!.GetValue(p)!.ToString() == userId.ToString())),
            Times.Once);
    }

    [Test]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<User>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync((User)null!);

        // Act
        var result = await _userRepository.GetByIdAsync(userId);

        // Assert
        Assert.That(result, Is.Null);

        _mockSql.Verify(x => x.QueryFirstOrDefaultAsync<User>(
                It.IsAny<string>(),
                It.Is<object>(p => p.GetType().GetProperty("Id")!.GetValue(p)!.ToString() == userId.ToString())),
            Times.Once);
    }

    [Test]
    public Task GetByIdAsync_WhenExceptionOccurs_ShouldLogAndRethrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exception = new Exception("Database error");

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<User>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _userRepository.GetByIdAsync(userId));
        Assert.That(ex.Message, Is.EqualTo("Database error"));

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        return Task.CompletedTask;
    }

    [Test]
    public async Task GetByEmailAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var email = "test@example.com";
        var expectedUser = CreateTestUser(Guid.NewGuid(), email);

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<User>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userRepository.GetByEmailAsync(email);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Email, Is.EqualTo(email));
        Assert.That(result.Id, Is.EqualTo(expectedUser.Id));

        _mockSql.Verify(x => x.QueryFirstOrDefaultAsync<User>(
                It.IsAny<string>(),
                It.Is<object>(p => p.GetType().GetProperty("Email")!.GetValue(p)!.ToString() == email)),
            Times.Once);
    }

    [Test]
    public async Task GetByUsernameAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var username = "testuser";
        var expectedUser = CreateTestUser(Guid.NewGuid(), "test@example.com", username);

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<User>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userRepository.GetByUsernameAsync(username);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Username, Is.EqualTo(username));
        Assert.That(result.Id, Is.EqualTo(expectedUser.Id));

        _mockSql.Verify(x => x.QueryFirstOrDefaultAsync<User>(
                It.IsAny<string>(),
                It.Is<object>(p => p.GetType().GetProperty("Username")!.GetValue(p)!.ToString() == username)),
            Times.Once);
    }

    [Test]
    public async Task CreateAsync_WhenSuccessful_ShouldReturnTrue()
    {
        // Arrange
        var user = CreateTestUser(Guid.NewGuid());

        _mockSql.Setup(x => x.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<User>()))
            .ReturnsAsync(1);


        // Act
        var result = await _userRepository.CreateAsync(user);

        // Assert
        Assert.That(result, Is.True);
        _mockSql.Verify(x => x.ExecuteAsync(It.IsAny<string>(), user), Times.Once);
    }

    [Test]
    public async Task CreateAsync_WhenExceptionOccurs_ShouldReturnFalse()
    {
        // Arrange
        var user = CreateTestUser(Guid.NewGuid());
        var exception = new Exception("Database error");

        _mockSql.Setup(x => x.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<User>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _userRepository.CreateAsync(user);

        // Assert
        Assert.That(result, Is.False);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task UpdateAsync_WhenSuccessful_ShouldReturnTrue()
    {
        // Arrange
        var user = CreateTestUser(Guid.NewGuid());

        _mockSql.Setup(x => x.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<User>()))
            .ReturnsAsync(1);


        // Act
        var result = await _userRepository.UpdateAsync(user);

        // Assert
        Assert.That(result, Is.True);
        _mockSql.Verify(x => x.ExecuteAsync(It.IsAny<string>(), user), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_WhenExceptionOccurs_ShouldReturnFalse()
    {
        // Arrange
        var user = CreateTestUser(Guid.NewGuid());
        var exception = new Exception("Database error");

        _mockSql.Setup(x => x.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<User>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _userRepository.UpdateAsync(user);

        // Assert
        Assert.That(result, Is.False);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task DeleteAsync_WhenSuccessful_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockSql.Setup(x => x.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<User>()))
            .ReturnsAsync(1);


        // Act
        var result = await _userRepository.DeleteAsync(userId);

        // Assert
        Assert.That(result, Is.True);
        _mockSql.Verify(x => x.ExecuteAsync(
                It.IsAny<string>(),
                It.Is<object>(p => p.GetType().GetProperty("Id")!.GetValue(p)!.ToString() == userId.ToString())),
            Times.Once);
    }

    [Test]
    public async Task DeleteAsync_WhenExceptionOccurs_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exception = new Exception("Database error");

        _mockSql.Setup(x => x.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _userRepository.DeleteAsync(userId);

        // Assert
        Assert.That(result, Is.False);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private static User CreateTestUser(Guid id, string email = "test@example.com", string username = "testuser")
    {
        return new User
        {
            Id = id,
            Email = email,
            PasswordHash = "hashedpassword",
            Username = username,
            FirstName = "Test",
            LastName = "User",
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastLogin = DateTime.UtcNow,
            ProfilePictureUrl = "https://example.com/profile.jpg",
            Bio = "Test bio"
        };
    }
}