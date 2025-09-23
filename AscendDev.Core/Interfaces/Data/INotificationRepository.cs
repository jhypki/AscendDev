using AscendDev.Core.Models.Notifications;

namespace AscendDev.Core.Interfaces.Data;

public interface INotificationRepository
{
    Task<Notification> CreateAsync(Notification notification);
    Task<Notification?> GetByIdAsync(Guid id);
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId);
    Task<int> GetUnreadCountByUserIdAsync(Guid userId);
    Task<Notification> UpdateAsync(Notification notification);
    Task<bool> MarkAsReadAsync(Guid id);
    Task<bool> MarkAllAsReadAsync(Guid userId);
    Task<bool> DeleteAsync(Guid id);
    Task<int> GetTotalCountByUserIdAsync(Guid userId);
}