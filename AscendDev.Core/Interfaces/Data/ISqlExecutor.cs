namespace AscendDev.Core.Interfaces.Data;

public interface ISqlExecutor
{
    Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null);
    Task<T> QueryFirstAsync<T>(string sql, object? parameters = null);
    Task<T> QuerySingleAsync<T>(string sql, object? parameters = null);
    Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null);
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null);
    Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(
        string sql,
        Func<TFirst, TSecond, TReturn> map,
        object? parameters = null,
        string splitOn = "Id");
    Task<int> ExecuteAsync(string sql, object? parameters = null);
}