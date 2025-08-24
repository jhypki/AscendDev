using System.Data;
using AscendDev.Core.Interfaces.Data;
using Dapper;

namespace AscendDev.Data;

public class DapperSqlExecutor<T>(IConnectionManager<T> connectionManager) : ISqlExecutor
    where T : class, IDbConnection
{
    public async Task<T1?> QueryFirstOrDefaultAsync<T1>(string sql, object? parameters = null)
    {
        using var connection = connectionManager.GetConnection();
        return await connection.QueryFirstOrDefaultAsync<T1>(sql, parameters);
    }

    public async Task<T1> QueryFirstAsync<T1>(string sql, object? parameters = null)
    {
        using var connection = connectionManager.GetConnection();
        return await connection.QueryFirstAsync<T1>(sql, parameters);
    }

    public async Task<T1> QuerySingleAsync<T1>(string sql, object? parameters = null)
    {
        using var connection = connectionManager.GetConnection();
        return await connection.QuerySingleAsync<T1>(sql, parameters);
    }

    public async Task<T1?> QuerySingleOrDefaultAsync<T1>(string sql, object? parameters = null)
    {
        using var connection = connectionManager.GetConnection();
        return await connection.QuerySingleOrDefaultAsync<T1>(sql, parameters);
    }

    public async Task<IEnumerable<T1>> QueryAsync<T1>(string sql, object? parameters = null)
    {
        using var connection = connectionManager.GetConnection();
        return await connection.QueryAsync<T1>(sql, parameters);
    }

    public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(
        string sql,
        Func<TFirst, TSecond, TReturn> map,
        object? parameters = null,
        string splitOn = "Id")
    {
        using var connection = connectionManager.GetConnection();
        return await connection.QueryAsync(sql, map, parameters, splitOn: splitOn);
    }

    public async Task<int> ExecuteAsync(string sql, object? parameters = null)
    {
        using var connection = connectionManager.GetConnection();
        return await connection.ExecuteAsync(sql, parameters);
    }
}