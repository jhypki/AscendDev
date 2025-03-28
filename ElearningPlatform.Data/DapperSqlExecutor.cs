using System.Data;
using Dapper;
using ElearningPlatform.Core.Interfaces.Data;

namespace ElearningPlatform.Data;

public class DapperSqlExecutor<T>(IConnectionManager<T> connectionManager) : ISqlExecutor
    where T : class, IDbConnection
{
    private readonly IConnectionManager<T> _connectionManager =
        connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));

    public async Task<T1?> QueryFirstOrDefaultAsync<T1>(string sql, object? parameters = null)
    {
        using var connection = _connectionManager.GetConnection();
        return await connection.QueryFirstOrDefaultAsync<T1>(sql, parameters);
    }

    public async Task<IEnumerable<T1>> QueryAsync<T1>(string sql, object? parameters = null)
    {
        using var connection = _connectionManager.GetConnection();
        return await connection.QueryAsync<T1>(sql, parameters);
    }

    public async Task<int> ExecuteAsync(string sql, object? parameters = null)
    {
        using var connection = _connectionManager.GetConnection();
        return await connection.ExecuteAsync(sql, parameters);
    }
}