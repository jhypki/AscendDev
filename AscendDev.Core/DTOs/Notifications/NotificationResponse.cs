using AscendDev.Core.Models.Notifications;
using System.Text.Json;

namespace AscendDev.Core.DTOs.Notifications;

public class NotificationResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public JsonDocument? Metadata { get; set; }
    public string? ActionUrl { get; set; }
    public bool IsRecent { get; set; }
    public TimeSpan Age { get; set; }
}