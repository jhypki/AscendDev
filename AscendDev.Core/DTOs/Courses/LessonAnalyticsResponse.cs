namespace AscendDev.Core.DTOs.Courses;

public class LessonAnalyticsResponse
{
    public string LessonId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public int CompletionCount { get; set; }
    public double AverageTimeSpent { get; set; }
    public double CompletionRate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastViewedAt { get; set; }
    public DateTime? LastCompletedAt { get; set; }

    // Engagement metrics
    public int UniqueViewers { get; set; }
    public int TotalTimeSpent { get; set; }
    public double AverageEngagementScore { get; set; }

    // Performance metrics
    public int DropOffCount { get; set; }
    public double DropOffRate { get; set; }
    public int RetryCount { get; set; }

    // Time-based analytics
    public Dictionary<string, int> ViewsByDay { get; set; } = new();
    public Dictionary<string, int> CompletionsByDay { get; set; } = new();

    // User feedback
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
    public int FeedbackCount { get; set; }
}