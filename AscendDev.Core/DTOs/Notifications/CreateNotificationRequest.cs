using AscendDev.Core.Models.Notifications;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace AscendDev.Core.DTOs.Notifications;

public class CreateNotificationRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public NotificationType Type { get; set; }

    [Required]
    [StringLength(500)]
    public string Title { get; set; } = null!;

    [Required]
    public string Message { get; set; } = null!;

    public JsonDocument? Metadata { get; set; }

    [StringLength(500)]
    public string? ActionUrl { get; set; }
}