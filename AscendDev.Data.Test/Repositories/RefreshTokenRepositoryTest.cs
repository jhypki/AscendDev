using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Auth;
using AscendDev.Data.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;

namespace AscendDev.Tests.Data.Repositories;

[TestFixture]
public class RefreshTokenRepositoryTest
{
    [SetUp]
    public void Setup()
    {
        _mockRedisConnection = new Mock<IConnectionManager<IDatabase>>();
        _mockRedisDb = new Mock<IDatabase>();
        _mockLogger = new Mock<ILogger<RefreshTokenRepository>>();

        _mockRedisConnection.Setup(x => x.GetConnection()).Returns(_mockRedisDb.Object);

        _refreshTokenRepository = new RefreshTokenRepository(_mockRedisConnection.Object, _mockLogger.Object);
    }

    private Mock<IConnectionManager<IDatabase>> _mockRedisConnection;
    private Mock<IDatabase> _mockRedisDb;
    private Mock<ILogger<RefreshTokenRepository>> _mockLogger;
    private RefreshTokenRepository _refreshTokenRepository;

    [Test]
    public async Task SaveAsync_WhenSuccessful_ShouldSaveToken()
    {
        // Arrange
        var refreshToken = CreateTestRefreshToken();
        var key = $"refresh:{refreshToken.Token}";

        _mockRedisDb.Setup(x => x.HashSetAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<HashEntry[]>(),
                It.IsAny<CommandFlags>()))
            .Returns(Task.CompletedTask);

        _mockRedisDb.Setup(x => x.KeyExpireAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<TimeSpan>(),
                It.IsAny<ExpireWhen>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _refreshTokenRepository.SaveAsync(refreshToken));

        _mockRedisDb.Verify(x => x.HashSetAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<HashEntry[]>(),
                It.IsAny<CommandFlags>()),
            Times.Once);

        _mockRedisDb.Verify(x => x.KeyExpireAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<TimeSpan>(),
                It.IsAny<ExpireWhen>(),
                It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Test]
    public Task SaveAsync_WhenExceptionOccurs_ShouldLogAndRethrow()
    {
        // Arrange
        var refreshToken = CreateTestRefreshToken();
        var exception = new Exception("Redis error");

        _mockRedisDb.Setup(x => x.HashSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<HashEntry[]>(),
                It.IsAny<CommandFlags>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _refreshTokenRepository.SaveAsync(refreshToken));
        Assert.That(ex.Message, Does.Contain("Error saving refresh token"));

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
    public async Task GetByTokenAsync_WhenTokenExists_ShouldReturnToken()
    {
        // Arrange
        var token = "test-token";
        var userId = Guid.NewGuid();
        var key = $"refresh:{token}";

        var hashEntries = new HashEntry[]
        {
            new("UserId", userId.ToString()),
            new("Token", token),
            new("Expires", DateTime.UtcNow.AddDays(7).ToString("o")),
            new("Created", DateTime.UtcNow.ToString("o")),
            new("CreatedByIp", "127.0.0.1"),
            new("Revoked", ""),
            new("RevokedByIp", "")
        };

        _mockRedisDb.Setup(x => x.HashGetAllAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(hashEntries);

        // Act
        var result = await _refreshTokenRepository.GetByTokenAsync(token);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Token, Is.EqualTo(token));
        Assert.That(result.UserId, Is.EqualTo(userId));
        Assert.That(result.IsActive, Is.True);

        _mockRedisDb.Verify(x => x.HashGetAllAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Test]
    public async Task GetByTokenAsync_WhenTokenDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var token = "test-token";
        var key = $"refresh:{token}";

        _mockRedisDb.Setup(x => x.HashGetAllAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(Array.Empty<HashEntry>());

        // Act
        var result = await _refreshTokenRepository.GetByTokenAsync(token);

        // Assert
        Assert.That(result, Is.Null);

        _mockRedisDb.Verify(x => x.HashGetAllAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Test]
    public Task GetByTokenAsync_WhenExceptionOccurs_ShouldLogAndRethrow()
    {
        // Arrange
        var token = "test-token";
        var exception = new Exception("Redis error");

        _mockRedisDb.Setup(x => x.HashGetAllAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<CommandFlags>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _refreshTokenRepository.GetByTokenAsync(token));
        Assert.That(ex.Message, Does.Contain("Error retrieving refresh token"));

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
    public async Task DeleteAsync_WhenSuccessful_ShouldDeleteToken()
    {
        // Arrange
        var token = "test-token";
        var key = $"refresh:{token}";

        _mockRedisDb.Setup(x => x.KeyDeleteAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _refreshTokenRepository.DeleteAsync(token));

        _mockRedisDb.Verify(x => x.KeyDeleteAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Test]
    public Task DeleteAsync_WhenExceptionOccurs_ShouldLogAndRethrow()
    {
        // Arrange
        var token = "test-token";
        var exception = new Exception("Redis error");

        _mockRedisDb.Setup(x => x.KeyDeleteAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<CommandFlags>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _refreshTokenRepository.DeleteAsync(token));
        Assert.That(ex.Message, Does.Contain("Error deleting refresh token"));

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
    public async Task RevokeAsync_WhenTokenExists_ShouldRevokeToken()
    {
        // Arrange
        var token = "test-token";
        var ip = "127.0.0.1";
        var userId = Guid.NewGuid();
        var key = $"refresh:{token}";

        var hashEntries = new HashEntry[]
        {
            new("UserId", userId.ToString()),
            new("Token", token),
            new("Expires", DateTime.UtcNow.AddDays(7).ToString("o")),
            new("Created", DateTime.UtcNow.ToString("o")),
            new("CreatedByIp", "127.0.0.1"),
            new("Revoked", ""),
            new("RevokedByIp", "")
        };

        _mockRedisDb.Setup(x => x.HashGetAllAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(hashEntries);

        _mockRedisDb.Setup(x => x.HashSetAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<HashEntry[]>(),
                It.IsAny<CommandFlags>()))
            .Returns(Task.CompletedTask);

        _mockRedisDb.Setup(x => x.KeyExpireAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<TimeSpan>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        var result = await _refreshTokenRepository.RevokeAsync(token, ip);

        // Assert
        Assert.That(result, Is.True);

        _mockRedisDb.Verify(x => x.HashSetAsync(
                It.Is<RedisKey>(k => k == key),
                It.Is<HashEntry[]>(entries =>
                    entries.Any(e => e.Name == "Revoked") &&
                    entries.Any(e => e.Name == "RevokedByIp" && e.Value == ip)),
                It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Test]
    public async Task RevokeAsync_WhenTokenDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var token = "test-token";
        var ip = "127.0.0.1";
        var key = $"refresh:{token}";

        _mockRedisDb.Setup(x => x.HashGetAllAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(Array.Empty<HashEntry>());

        // Act
        var result = await _refreshTokenRepository.RevokeAsync(token, ip);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public Task RevokeAsync_WhenExceptionOccurs_ShouldLogAndRethrow()
    {
        // Arrange
        var token = "test-token";
        var ip = "127.0.0.1";
        var exception = new Exception("Redis error");

        _mockRedisDb.Setup(x => x.HashGetAllAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<CommandFlags>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _refreshTokenRepository.RevokeAsync(token, ip));
        Assert.That(ex.Message, Does.Contain("Error revoking refresh token"));

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

    private static RefreshToken CreateTestRefreshToken()
    {
        return new RefreshToken
        {
            UserId = Guid.NewGuid(),
            Token = Guid.NewGuid().ToString(),
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
            CreatedByIp = "127.0.0.1"
        };
    }
}