using AscendDev.Core.Models.Auth;

namespace AscendDev.Core.Models.Analytics;

public class UserStatistics
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int TotalPoints { get; set; } = 0;
    public int LessonsCompleted { get; set; } = 0;
    public int CoursesCompleted { get; set; } = 0;
    public int CurrentStreak { get; set; } = 0;
    public int LongestStreak { get; set; } = 0;
    public DateTime? LastActivityDate { get; set; }
    public int TotalCodeReviews { get; set; } = 0;
    public int TotalDiscussions { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;

    // Computed Properties
    public bool IsActiveToday => LastActivityDate?.Date == DateTime.UtcNow.Date;
    public bool IsActiveThisWeek => LastActivityDate >= DateTime.UtcNow.AddDays(-7);
    public TimeSpan? TimeSinceLastActivity => LastActivityDate.HasValue ?
        DateTime.UtcNow - LastActivityDate.Value : null;
    public double AveragePointsPerLesson => LessonsCompleted > 0 ?
        (double)TotalPoints / LessonsCompleted : 0;
    public int TotalSocialInteractions => TotalCodeReviews + TotalDiscussions;
}