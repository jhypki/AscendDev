using AscendDev.Core.Models.Auth;
using System.Text.Json;

namespace AscendDev.Core.Models.Notifications;

public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public JsonDocument? Metadata { get; set; }
    public string? ActionUrl { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;

    // Computed Properties
    public bool IsRecent => CreatedAt > DateTime.UtcNow.AddHours(-24);
    public TimeSpan Age => DateTime.UtcNow - CreatedAt;
}

public enum NotificationType
{
    Achievement,
    Progress,
    Discussion,
    CodeReview,
    System,
    Welcome,
    Reminder,
    Social
}