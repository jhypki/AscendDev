using AscendDev.Core.Models.Auth;
using System.Text.Json;

namespace AscendDev.Core.Models.Gamification;

public class UserAchievement
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid AchievementId { get; set; }
    public DateTime EarnedAt { get; set; }
    public JsonDocument? ProgressData { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;
    public Achievement Achievement { get; set; } = null!;

    // Computed Properties
    public bool IsRecent => EarnedAt > DateTime.UtcNow.AddDays(-7);
}