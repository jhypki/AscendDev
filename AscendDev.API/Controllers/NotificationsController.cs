using AscendDev.Core.DTOs.Notifications;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AscendDev.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID");
        }
        return userId;
    }

    /// <summary>
    /// Get notifications for the current user
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationResponse>>> GetNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            var notifications = await _notificationService.GetByUserIdAsync(userId, page, pageSize);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications for user");
            return StatusCode(500, "An error occurred while retrieving notifications");
        }
    }

    /// <summary>
    /// Get unread notifications for the current user
    /// </summary>
    [HttpGet("unread")]
    public async Task<ActionResult<IEnumerable<NotificationResponse>>> GetUnreadNotifications()
    {
        try
        {
            var userId = GetCurrentUserId();
            var notifications = await _notificationService.GetUnreadByUserIdAsync(userId);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread notifications for user");
            return StatusCode(500, "An error occurred while retrieving unread notifications");
        }
    }

    /// <summary>
    /// Get unread notification count for the current user
    /// </summary>
    [HttpGet("unread/count")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        try
        {
            var userId = GetCurrentUserId();
            var count = await _notificationService.GetUnreadCountByUserIdAsync(userId);
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread notification count for user");
            return StatusCode(500, "An error occurred while retrieving unread notification count");
        }
    }

    /// <summary>
    /// Mark a notification as read
    /// </summary>
    [HttpPut("{id}/read")]
    public async Task<ActionResult> MarkAsRead(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _notificationService.MarkAsReadAsync(id, userId);

            if (!success)
            {
                return NotFound("Notification not found or does not belong to the current user");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read", id);
            return StatusCode(500, "An error occurred while marking the notification as read");
        }
    }

    /// <summary>
    /// Mark all notifications as read for the current user
    /// </summary>
    [HttpPut("read-all")]
    public async Task<ActionResult> MarkAllAsRead()
    {
        try
        {
            var userId = GetCurrentUserId();
            await _notificationService.MarkAllAsReadAsync(userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read for user");
            return StatusCode(500, "An error occurred while marking all notifications as read");
        }
    }

    /// <summary>
    /// Delete a notification
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteNotification(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _notificationService.DeleteAsync(id, userId);

            if (!success)
            {
                return NotFound("Notification not found or does not belong to the current user");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification {NotificationId}", id);
            return StatusCode(500, "An error occurred while deleting the notification");
        }
    }

    /// <summary>
    /// Get total notification count for the current user
    /// </summary>
    [HttpGet("count")]
    public async Task<ActionResult<int>> GetTotalCount()
    {
        try
        {
            var userId = GetCurrentUserId();
            var count = await _notificationService.GetTotalCountByUserIdAsync(userId);
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total notification count for user");
            return StatusCode(500, "An error occurred while retrieving total notification count");
        }
    }

    /// <summary>
    /// Create a test notification (for debugging purposes)
    /// </summary>
    [HttpPost("test")]
    public async Task<ActionResult> CreateTestNotification()
    {
        try
        {
            var userId = GetCurrentUserId();

            await _notificationService.SendNotificationAsync(
                userId,
                NotificationType.System,
                "Test Notification",
                "This is a test notification to verify the system is working.",
                null,
                new { test = true }
            );

            return Ok(new { message = "Test notification created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating test notification");
            return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }
}