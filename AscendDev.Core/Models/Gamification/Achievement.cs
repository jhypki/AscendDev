using System.Text.Json;

namespace AscendDev.Core.Models.Gamification;

public class Achievement
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? IconUrl { get; set; }
    public JsonDocument Criteria { get; set; } = null!;
    public int Points { get; set; } = 0;
    public AchievementCategory Category { get; set; } = AchievementCategory.General;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public List<UserAchievement> UserAchievements { get; set; } = new();

    // Computed Properties
    public int EarnedCount => UserAchievements.Count;
}

public enum AchievementCategory
{
    General,
    Progress,
    Social,
    Skill,
    Streak,
    Community
}