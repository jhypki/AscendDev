using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Notifications;
using Microsoft.Extensions.Logging;

namespace AscendDev.Data.Repositories;

public class NotificationRepository(ISqlExecutor sql, ILogger<NotificationRepository> logger) : INotificationRepository
{

    public async Task<Notification> CreateAsync(Notification notification)
    {
        const string query = @"
            INSERT INTO notifications (id, user_id, type, title, message, is_read, created_at, read_at, metadata, action_url)
            VALUES (@Id, @UserId, @Type, @Title, @Message, @IsRead, @CreatedAt, @ReadAt, @Metadata::jsonb, @ActionUrl)
            RETURNING *";

        try
        {
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

            var result = await sql.QuerySingleAsync<Notification>(query, parameters);
            return result;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating notification");
            throw;
        }
    }

    public async Task<Notification?> GetByIdAsync(Guid id)
    {
        const string query = "SELECT * FROM notifications WHERE id = @Id";

        try
        {
            return await sql.QuerySingleOrDefaultAsync<Notification>(query, new { Id = id });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting notification by id {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        const string query = @"
            SELECT * FROM notifications
            WHERE user_id = @UserId
            ORDER BY created_at DESC
            LIMIT @PageSize OFFSET @Offset";

        var offset = (page - 1) * pageSize;

        try
        {
            return await sql.QueryAsync<Notification>(query, new { UserId = userId, PageSize = pageSize, Offset = offset });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting notifications by user id {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId)
    {
        const string query = @"
            SELECT * FROM notifications
            WHERE user_id = @UserId AND is_read = false
            ORDER BY created_at DESC";

        try
        {
            return await sql.QueryAsync<Notification>(query, new { UserId = userId });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting unread notifications by user id {UserId}", userId);
            throw;
        }
    }

    public async Task<int> GetUnreadCountByUserIdAsync(Guid userId)
    {
        const string query = "SELECT COUNT(*) FROM notifications WHERE user_id = @UserId AND is_read = false";

        try
        {
            return await sql.QuerySingleAsync<int>(query, new { UserId = userId });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting unread count by user id {UserId}", userId);
            throw;
        }
    }

    public async Task<Notification> UpdateAsync(Notification notification)
    {
        const string query = @"
            UPDATE notifications
            SET type = @Type, title = @Title, message = @Message, is_read = @IsRead,
                read_at = @ReadAt, metadata = @Metadata::jsonb, action_url = @ActionUrl
            WHERE id = @Id
            RETURNING *";

        try
        {
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

            return await sql.QuerySingleAsync<Notification>(query, parameters);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating notification {Id}", notification.Id);
            throw;
        }
    }

    public async Task<bool> MarkAsReadAsync(Guid id)
    {
        const string query = @"
            UPDATE notifications
            SET is_read = true, read_at = @ReadAt
            WHERE id = @Id";

        try
        {
            var rowsAffected = await sql.ExecuteAsync(query, new { Id = id, ReadAt = DateTime.UtcNow });
            return rowsAffected > 0;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error marking notification as read {Id}", id);
            throw;
        }
    }

    public async Task<bool> MarkAllAsReadAsync(Guid userId)
    {
        const string query = @"
            UPDATE notifications
            SET is_read = true, read_at = @ReadAt
            WHERE user_id = @UserId AND is_read = false";

        try
        {
            var rowsAffected = await sql.ExecuteAsync(query, new { UserId = userId, ReadAt = DateTime.UtcNow });
            return rowsAffected > 0;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error marking all notifications as read for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        const string query = "DELETE FROM notifications WHERE id = @Id";

        try
        {
            var rowsAffected = await sql.ExecuteAsync(query, new { Id = id });
            return rowsAffected > 0;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting notification {Id}", id);
            throw;
        }
    }

    public async Task<int> GetTotalCountByUserIdAsync(Guid userId)
    {
        const string query = "SELECT COUNT(*) FROM notifications WHERE user_id = @UserId";

        try
        {
            return await sql.QuerySingleAsync<int>(query, new { UserId = userId });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting total count by user id {UserId}", userId);
            throw;
        }
    }
}