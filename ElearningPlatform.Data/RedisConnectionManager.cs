// ElearningPlatform.Services/Data/Redis/RedisConnectionManager.cs

using ElearningPlatform.Core.Interfaces.Data;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace ElearningPlatform.Data;

public class RedisConnectionManager : IConnectionManager<IDatabase>, IDisposable
{
    private readonly Lazy<ConnectionMultiplexer> _connection;
    private bool _disposed;

    public RedisConnectionManager(IConfiguration configuration)
    {
        var connectionString = configuration["RedisConnectionString"]
                               ?? throw new ArgumentException(
                                   "RedisConnectionString must be provided in configuration.");

        _connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(connectionString));
    }

    public IDatabase GetConnection()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RedisConnectionManager));

        return _connection.Value.GetDatabase();
    }

    public void Dispose()
    {
        if (!_disposed && _connection.IsValueCreated) _connection.Value.Dispose();
        _disposed = true;
    }
}