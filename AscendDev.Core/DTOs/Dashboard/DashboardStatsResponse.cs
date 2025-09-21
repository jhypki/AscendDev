using System.Text.Json.Serialization;

namespace AscendDev.Core.DTOs.Dashboard;

public class DashboardStatsResponse
{
    [JsonPropertyName("totalCourses")]
    public int TotalCourses { get; set; }

    [JsonPropertyName("completedCourses")]
    public int CompletedCourses { get; set; }

    [JsonPropertyName("inProgressCourses")]
    public int InProgressCourses { get; set; }

    [JsonPropertyName("totalLessons")]
    public int TotalLessons { get; set; }

    [JsonPropertyName("completedLessons")]
    public int CompletedLessons { get; set; }

    [JsonPropertyName("streakDays")]
    public int StreakDays { get; set; }

    [JsonPropertyName("totalStudyTime")]
    public int TotalStudyTime { get; set; } // in minutes

    [JsonPropertyName("recentActivity")]
    public List<RecentActivityResponse> RecentActivity { get; set; } = new();
}

public class RecentActivityResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty; // lesson_completed, course_started, course_completed

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("courseTitle")]
    public string? CourseTitle { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}

public class UserProgressResponse
{
    [JsonPropertyName("courseId")]
    public string CourseId { get; set; } = string.Empty;

    [JsonPropertyName("courseTitle")]
    public string CourseTitle { get; set; } = string.Empty;

    [JsonPropertyName("totalLessons")]
    public int TotalLessons { get; set; }

    [JsonPropertyName("completedLessons")]
    public int CompletedLessons { get; set; }

    [JsonPropertyName("completionPercentage")]
    public int CompletionPercentage { get; set; }

    [JsonPropertyName("lastAccessed")]
    public DateTime LastAccessed { get; set; }
}

public class LearningStreakResponse
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("completed")]
    public int Completed { get; set; }
}