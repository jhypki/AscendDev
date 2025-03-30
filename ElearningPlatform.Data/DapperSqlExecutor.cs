namespace ElearningPlatform.Data;

using System.Data;
using Core.Interfaces.Data;
using Dapper;

public class DapperSqlExecutor<T>(IConnectionManager<T> connectionManager) : ISqlExecutor
    where T : class, IDbConnection
{
    public async Task<T1?> QueryFirstOrDefaultAsync<T1>(string sql, object? parameters = null)
    {
        using var connection = connectionManager.GetConnection();
        return await connection.QueryFirstOrDefaultAsync<T1>(sql, parameters);
    }

    public async Task<IEnumerable<T1>> QueryAsync<T1>(string sql, object? parameters = null)
    {
        using var connection = connectionManager.GetConnection();
        return await connection.QueryAsync<T1>(sql, parameters);
    }

    public async Task<int> ExecuteAsync(string sql, object? parameters = null)
    {
        using var connection = connectionManager.GetConnection();
        return await connection.ExecuteAsync(sql, parameters);
    }
}
