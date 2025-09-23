using AscendDev.Core.DTOs.Notifications;
using AscendDev.Core.Models.Notifications;

namespace AscendDev.Core.Interfaces.Services;

public interface INotificationService
{
    Task<NotificationResponse> CreateAsync(CreateNotificationRequest request);
    Task<NotificationResponse?> GetByIdAsync(Guid id);
    Task<IEnumerable<NotificationResponse>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<IEnumerable<NotificationResponse>> GetUnreadByUserIdAsync(Guid userId);
    Task<int> GetUnreadCountByUserIdAsync(Guid userId);
    Task<bool> MarkAsReadAsync(Guid id, Guid userId);
    Task<bool> MarkAllAsReadAsync(Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid userId);
    Task<int> GetTotalCountByUserIdAsync(Guid userId);

    // Real-time notification methods
    Task SendNotificationAsync(Guid userId, NotificationType type, string title, string message, string? actionUrl = null, object? metadata = null);
    Task SendCodeReviewNotificationAsync(Guid revieweeId, Guid reviewerId, string reviewerName, string lessonTitle, Guid codeReviewId, int submissionId);
}