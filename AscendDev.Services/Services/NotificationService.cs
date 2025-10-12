using AscendDev.Core.DTOs.Notifications;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.Notifications;
using AscendDev.Core.Models.Auth;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AscendDev.Services.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationHubService _hubService;
    private readonly IEmailService _emailService;
    private readonly IUserRepository _userRepository;
    private readonly IUserSettingsRepository _userSettingsRepository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        INotificationHubService hubService,
        IEmailService emailService,
        IUserRepository userRepository,
        IUserSettingsRepository userSettingsRepository,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _hubService = hubService;
        _emailService = emailService;
        _userRepository = userRepository;
        _userSettingsRepository = userSettingsRepository;
        _logger = logger;
    }

    public async Task<NotificationResponse> CreateAsync(CreateNotificationRequest request)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Type = request.Type,
            Title = request.Title,
            Message = request.Message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            Metadata = request.Metadata,
            ActionUrl = request.ActionUrl
        };

        var createdNotification = await _notificationRepository.CreateAsync(notification);
        return MapToResponse(createdNotification);
    }

    public async Task<NotificationResponse?> GetByIdAsync(Guid id)
    {
        var notification = await _notificationRepository.GetByIdAsync(id);
        return notification != null ? MapToResponse(notification) : null;
    }

    public async Task<IEnumerable<NotificationResponse>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(userId, page, pageSize);
        return notifications.Select(MapToResponse);
    }

    public async Task<IEnumerable<NotificationResponse>> GetUnreadByUserIdAsync(Guid userId)
    {
        var notifications = await _notificationRepository.GetUnreadByUserIdAsync(userId);
        return notifications.Select(MapToResponse);
    }

    public async Task<int> GetUnreadCountByUserIdAsync(Guid userId)
    {
        return await _notificationRepository.GetUnreadCountByUserIdAsync(userId);
    }

    public async Task<bool> MarkAsReadAsync(Guid id, Guid userId)
    {
        // Verify the notification belongs to the user
        var notification = await _notificationRepository.GetByIdAsync(id);
        if (notification == null || notification.UserId != userId)
            return false;

        return await _notificationRepository.MarkAsReadAsync(id);
    }

    public async Task<bool> MarkAllAsReadAsync(Guid userId)
    {
        return await _notificationRepository.MarkAllAsReadAsync(userId);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        // Verify the notification belongs to the user
        var notification = await _notificationRepository.GetByIdAsync(id);
        if (notification == null || notification.UserId != userId)
            return false;

        return await _notificationRepository.DeleteAsync(id);
    }

    public async Task<int> GetTotalCountByUserIdAsync(Guid userId)
    {
        return await _notificationRepository.GetTotalCountByUserIdAsync(userId);
    }

    public async Task SendNotificationAsync(Guid userId, NotificationType type, string title, string message, string? actionUrl = null, object? metadata = null)
    {
        try
        {
            // Create notification in database
            var request = new CreateNotificationRequest
            {
                UserId = userId,
                Type = type,
                Title = title,
                Message = message,
                ActionUrl = actionUrl,
                Metadata = metadata != null ? JsonSerializer.SerializeToDocument(metadata) : null
            };

            var notificationResponse = await CreateAsync(request);

            // Send real-time notification via SignalR
            await _hubService.SendNotificationToUserAsync(userId, notificationResponse);

            _logger.LogInformation("Notification sent to user {UserId}: {Title}", userId, title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to user {UserId}: {Title}", userId, title);
            throw;
        }
    }

    public async Task SendCodeReviewNotificationAsync(Guid revieweeId, Guid reviewerId, string reviewerName, string lessonTitle, Guid codeReviewId, int submissionId)
    {
        var title = "New Code Review";
        var message = $"{reviewerName} has reviewed your submission for '{lessonTitle}'";
        var actionUrl = $"/submissions/{submissionId}/review";

        var metadata = new
        {
            ReviewerId = reviewerId,
            ReviewerName = reviewerName,
            LessonTitle = lessonTitle,
            CodeReviewId = codeReviewId,
            SubmissionId = submissionId
        };

        // Send in-app notification
        await SendNotificationAsync(revieweeId, NotificationType.CodeReview, title, message, actionUrl, metadata);

        // Send email notification if user has enabled it
        await SendEmailNotificationIfEnabledAsync(revieweeId, async (user, userSettings) =>
        {
            if (userSettings.EmailOnCodeReview)
            {
                var fullActionUrl = $"https://ascenddev.com{actionUrl}"; // TODO: Get base URL from configuration
                await _emailService.SendCodeReviewNotificationEmailAsync(user.Email, reviewerName, lessonTitle, fullActionUrl);
            }
        });
    }

    private async Task SendEmailNotificationIfEnabledAsync(Guid userId, Func<User, UserSettings, Task> emailAction)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found for email notification: {UserId}", userId);
                return;
            }

            var userSettings = await _userSettingsRepository.GetByUserIdAsync(userId);
            if (userSettings == null)
            {
                _logger.LogWarning("User settings not found for email notification: {UserId}", userId);
                return;
            }

            await emailAction(user, userSettings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email notification to user {UserId}", userId);
            // Don't throw - email failures shouldn't break the notification flow
        }
    }

    private static NotificationResponse MapToResponse(Notification notification)
    {
        return new NotificationResponse
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Type = notification.Type,
            Title = notification.Title,
            Message = notification.Message,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            ReadAt = notification.ReadAt,
            Metadata = notification.Metadata,
            ActionUrl = notification.ActionUrl,
            IsRecent = notification.IsRecent,
            Age = notification.Age
        };
    }
}