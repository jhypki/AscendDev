// AscendDev.Services/Data/Redis/RefreshTokenRepository.cs

using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Auth;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AscendDev.Data.Repositories;

public class RefreshTokenRepository(
    IConnectionManager<IDatabase> redisConnection,
    ILogger<RefreshTokenRepository> logger)
    : IRefreshTokenRepository
{
    public async Task SaveAsync(RefreshToken refreshToken)
    {
        try
        {
            var db = redisConnection.GetConnection();
            var key = $"refresh:{refreshToken.Token}";

            var hashEntries = new HashEntry[]
            {
                new("UserId", refreshToken.UserId.ToString()),
                new("Token", refreshToken.Token),
                new("Expires", refreshToken.Expires.ToString("o")),
                new("Created", refreshToken.Created.ToString("o")),
                new("CreatedByIp", refreshToken.CreatedByIp),
                new("Revoked", refreshToken.Revoked?.ToString("o") ?? ""),
                new("RevokedByIp", refreshToken.RevokedByIp ?? "")
            };

            await db.HashSetAsync(key, hashEntries);
            await db.KeyExpireAsync(key, refreshToken.Expires - DateTime.UtcNow);

            logger.LogInformation("Saved refresh token {Token} for user {UserId}", refreshToken.Token,
                refreshToken.UserId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save refresh token {Token}", refreshToken.Token);
            throw new Exception($"Error saving refresh token {refreshToken.Token}", ex);
        }
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        try
        {
            var db = redisConnection.GetConnection();
            var key = $"refresh:{token}";

            var entries = await db.HashGetAllAsync(key);
            if (entries.Length == 0)
            {
                logger.LogWarning("Refresh token {Token} not found in Redis", token);
                return null;
            }

            var refreshToken = new RefreshToken
            {
                UserId = Guid.Parse(entries.First(e => e.Name == "UserId").Value),
                Token = entries.First(e => e.Name == "Token").Value,
                Expires = DateTime.Parse(entries.First(e => e.Name == "Expires").Value),
                Created = DateTime.Parse(entries.First(e => e.Name == "Created").Value),
                CreatedByIp = entries.First(e => e.Name == "CreatedByIp").Value,
                Revoked = entries.First(e => e.Name == "Revoked").Value.HasValue
                    ? DateTime.Parse(entries.First(e => e.Name == "Revoked").Value)
                    : null,
                RevokedByIp = entries.First(e => e.Name == "RevokedByIp").Value.HasValue
                    ? (string?)entries.First(e => e.Name == "RevokedByIp").Value
                    : null
            };

            if (!refreshToken.IsActive)
            {
                logger.LogWarning("Refresh token {Token} is inactive or expired", token);
                return null;
            }

            logger.LogInformation("Retrieved active refresh token {Token}", token);
            return refreshToken;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve refresh token {Token}", token);
            throw new Exception($"Error retrieving refresh token {token}", ex);
        }
    }

    public async Task DeleteAsync(string token)
    {
        try
        {
            var db = redisConnection.GetConnection();
            var key = $"refresh:{token}";

            var result = await db.KeyDeleteAsync(key);
            if (result)
                logger.LogInformation("Deleted refresh token {Token}", token);
            else
                logger.LogWarning("Refresh token {Token} not found for deletion", token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete refresh token {Token}", token);
            throw new Exception($"Error deleting refresh token {token}", ex);
        }
    }

    public async Task DeleteByUserIdAsync(Guid userId)
    {
        try
        {
            var db = redisConnection.GetConnection();
            const string pattern = "refresh:*";
            var server = db.Multiplexer.GetServer(db.Multiplexer.GetEndPoints()[0]);
            var keys = server.Keys(pattern: pattern);

            foreach (var key in keys)
            {
                var entries = await db.HashGetAllAsync(key);
                var storedUserId = Guid.Parse(entries.First(e => e.Name == "UserId").Value);
                if (storedUserId == userId) await db.KeyDeleteAsync(key);
            }

            logger.LogInformation("Deleted refresh tokens for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete refresh tokens for user {UserId}", userId);
            throw new Exception($"Error deleting refresh tokens for user {userId}", ex);
        }
    }

    public async Task<bool> RevokeAsync(string token, string ip)
    {
        try
        {
            var db = redisConnection.GetConnection();
            var key = $"refresh:{token}";

            var entries = await db.HashGetAllAsync(key);
            if (entries.Length == 0)
            {
                logger.LogWarning("Refresh token {Token} not found in Redis", token);
                return false;
            }

            var refreshToken = new RefreshToken
            {
                UserId = Guid.Parse(entries.First(e => e.Name == "UserId").Value),
                Token = entries.First(e => e.Name == "Token").Value,
                Expires = DateTime.Parse(entries.First(e => e.Name == "Expires").Value),
                Created = DateTime.Parse(entries.First(e => e.Name == "Created").Value),
                CreatedByIp = entries.First(e => e.Name == "CreatedByIp").Value,
                Revoked = DateTime.UtcNow,
                RevokedByIp = ip
            };

            var hashEntries = new HashEntry[]
            {
                new("Revoked", refreshToken.Revoked.Value.ToString("o")),
                new("RevokedByIp", refreshToken.RevokedByIp)
            };

            await db.HashSetAsync(key, hashEntries);
            await db.KeyExpireAsync(key, refreshToken.Expires - DateTime.UtcNow);

            logger.LogInformation("Revoked refresh token {Token}", token);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to revoke refresh token {Token}", token);
            throw new Exception($"Error revoking refresh token {token}", ex);
        }
    }
}