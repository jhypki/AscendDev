using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Auth;
using AscendDev.Data.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace AscendDev.Tests.Data.Repositories;

[TestFixture]
public class UserRepositoryExternalIdTest
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
    public async Task GetByExternalIdAndProviderAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var externalId = "oauth-123456";
        var provider = "google";
        var expectedUser = CreateTestUserWithExternalId(Guid.NewGuid(), externalId, provider);

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<User>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userRepository.GetByExternalIdAndProviderAsync(externalId, provider);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ExternalId, Is.EqualTo(externalId));
        Assert.That(result.Provider, Is.EqualTo(provider));

        _mockSql.Verify(x => x.QueryFirstOrDefaultAsync<User>(
                It.IsAny<string>(),
                It.Is<object>(p =>
                    p.GetType().GetProperty("ExternalId")!.GetValue(p)!.ToString() == externalId &&
                    p.GetType().GetProperty("Provider")!.GetValue(p)!.ToString() == provider)),
            Times.Once);
    }

    [Test]
    public async Task GetByExternalIdAndProviderAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var externalId = "oauth-123456";
        var provider = "google";

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<User>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync((User)null!);

        // Act
        var result = await _userRepository.GetByExternalIdAndProviderAsync(externalId, provider);

        // Assert
        Assert.That(result, Is.Null);

        _mockSql.Verify(x => x.QueryFirstOrDefaultAsync<User>(
                It.IsAny<string>(),
                It.Is<object>(p =>
                    p.GetType().GetProperty("ExternalId")!.GetValue(p)!.ToString() == externalId &&
                    p.GetType().GetProperty("Provider")!.GetValue(p)!.ToString() == provider)),
            Times.Once);
    }

    [Test]
    public Task GetByExternalIdAndProviderAsync_WhenExceptionOccurs_ShouldLogAndRethrow()
    {
        // Arrange
        var externalId = "oauth-123456";
        var provider = "google";
        var exception = new Exception("Database error");

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<User>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () =>
            await _userRepository.GetByExternalIdAndProviderAsync(externalId, provider));
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

    private static User CreateTestUserWithExternalId(Guid id, string externalId, string provider,
        string email = "test@example.com", string username = "testuser")
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
            Bio = "Test bio",
            ExternalId = externalId,
            Provider = provider
        };
    }
}