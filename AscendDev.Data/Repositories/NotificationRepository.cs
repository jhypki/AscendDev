using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Notifications;
using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;

namespace AscendDev.Data.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly string _connectionString;

    public NotificationRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<Notification> CreateAsync(Notification notification)
    {
        const string sql = @"
            INSERT INTO notifications (id, user_id, type, title, message, is_read, created_at, read_at, metadata, action_url)
            VALUES (@Id, @UserId, @Type, @Title, @Message, @IsRead, @CreatedAt, @ReadAt, @Metadata::jsonb, @ActionUrl)
            RETURNING *";

        using var connection = new NpgsqlConnection(_connectionString);

        var parameters = new
        {
            notification.Id,
            notification.UserId,
            Type = notification.Type.ToString(),
            notification.Title,
            notification.Message,
            notification.IsRead,
            notification.CreatedAt,
            notification.ReadAt,
            Metadata = notification.Metadata?.RootElement.GetRawText(),
            notification.ActionUrl
        };

        var result = await connection.QuerySingleAsync<Notification>(sql, parameters);
        return result;
    }

    public async Task<Notification?> GetByIdAsync(Guid id)
    {
        const string sql = "SELECT * FROM notifications WHERE id = @Id";

        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<Notification>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        const string sql = @"
            SELECT * FROM notifications 
            WHERE user_id = @UserId 
            ORDER BY created_at DESC 
            LIMIT @PageSize OFFSET @Offset";

        var offset = (page - 1) * pageSize;

        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<Notification>(sql, new { UserId = userId, PageSize = pageSize, Offset = offset });
    }

    public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId)
    {
        const string sql = @"
            SELECT * FROM notifications 
            WHERE user_id = @UserId AND is_read = false 
            ORDER BY created_at DESC";

        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<Notification>(sql, new { UserId = userId });
    }

    public async Task<int> GetUnreadCountByUserIdAsync(Guid userId)
    {
        const string sql = "SELECT COUNT(*) FROM notifications WHERE user_id = @UserId AND is_read = false";

        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QuerySingleAsync<int>(sql, new { UserId = userId });
    }

    public async Task<Notification> UpdateAsync(Notification notification)
    {
        const string sql = @"
            UPDATE notifications
            SET type = @Type, title = @Title, message = @Message, is_read = @IsRead,
                read_at = @ReadAt, metadata = @Metadata::jsonb, action_url = @ActionUrl
            WHERE id = @Id
            RETURNING *";

        using var connection = new NpgsqlConnection(_connectionString);

        var parameters = new
        {
            notification.Id,
            Type = notification.Type.ToString(),
            notification.Title,
            notification.Message,
            notification.IsRead,
            notification.ReadAt,
            Metadata = notification.Metadata?.RootElement.GetRawText(),
            notification.ActionUrl
        };

        return await connection.QuerySingleAsync<Notification>(sql, parameters);
    }

    public async Task<bool> MarkAsReadAsync(Guid id)
    {
        const string sql = @"
            UPDATE notifications 
            SET is_read = true, read_at = @ReadAt 
            WHERE id = @Id";

        using var connection = new NpgsqlConnection(_connectionString);
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, ReadAt = DateTime.UtcNow });
        return rowsAffected > 0;
    }

    public async Task<bool> MarkAllAsReadAsync(Guid userId)
    {
        const string sql = @"
            UPDATE notifications 
            SET is_read = true, read_at = @ReadAt 
            WHERE user_id = @UserId AND is_read = false";

        using var connection = new NpgsqlConnection(_connectionString);
        var rowsAffected = await connection.ExecuteAsync(sql, new { UserId = userId, ReadAt = DateTime.UtcNow });
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        const string sql = "DELETE FROM notifications WHERE id = @Id";

        using var connection = new NpgsqlConnection(_connectionString);
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<int> GetTotalCountByUserIdAsync(Guid userId)
    {
        const string sql = "SELECT COUNT(*) FROM notifications WHERE user_id = @UserId";

        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QuerySingleAsync<int>(sql, new { UserId = userId });
    }
}