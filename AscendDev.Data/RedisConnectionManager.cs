using AscendDev.Core.Interfaces.Data;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace AscendDev.Data;

public class RedisConnectionManager : IConnectionManager<IDatabase>, IDisposable
{
    private readonly Lazy<ConnectionMultiplexer> _connection;
    private bool _disposed;

    public RedisConnectionManager(IConfiguration configuration)
    {
        var redisSection = configuration.GetSection("Redis");
        var host = redisSection["Host"] ?? "localhost";
        var port = int.Parse(redisSection["Port"] ?? "6379");
        var password = redisSection["Password"] ?? string.Empty;
        var user = redisSection["User"] ?? "default";

        _connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(
            new ConfigurationOptions
            {
                EndPoints = { { host, port } },
                User = user,
                Password = password,
                AbortOnConnectFail = false,  // Add this line
                ConnectTimeout = 5000,       // Increase timeout to 5 seconds
                ConnectRetry = 3
            }
        ));
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