namespace ElearningPlatform.Core.Interfaces.Data;

public interface ISqlExecutor
{
    Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null);
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null);
    Task<int> ExecuteAsync(string sql, object? parameters = null);
}