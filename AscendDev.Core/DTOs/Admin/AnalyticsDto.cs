namespace AscendDev.Core.DTOs.Admin;

public class DashboardStatisticsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsersToday { get; set; }
    public int ActiveUsersThisWeek { get; set; }
    public int ActiveUsersThisMonth { get; set; }
    public int LessonsCompletedToday { get; set; }
    public int LessonsCompletedThisWeek { get; set; }
    public int LessonsCompletedThisMonth { get; set; }
    public int CoursesPublished { get; set; }
    public int TotalLessons { get; set; }
    public int TotalDiscussions { get; set; }
    public double AverageSessionDurationMinutes { get; set; }
    public List<PopularCourseDto> PopularCourses { get; set; } = new();
    public List<UserActivityTrendDto> UserActivityTrend { get; set; } = new();
    public List<LessonCompletionTrendDto> LessonCompletionTrend { get; set; } = new();
}

public class PopularCourseDto
{
    public string Id { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Language { get; set; } = null!;
    public int Completions { get; set; }
    public int ActiveStudents { get; set; }
    public double AverageRating { get; set; }
}

public class UserActivityTrendDto
{
    public DateTime Date { get; set; }
    public int ActiveUsers { get; set; }
    public int NewRegistrations { get; set; }
    public int LoginCount { get; set; }
}

public class LessonCompletionTrendDto
{
    public DateTime Date { get; set; }
    public int CompletedLessons { get; set; }
    public int UniqueUsers { get; set; }
}

public class UserEngagementReportDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public double EngagementRate { get; set; }
    public List<UserEngagementDetailDto> TopEngagedUsers { get; set; } = new();
    public List<UserEngagementDetailDto> LeastEngagedUsers { get; set; } = new();
}

public class UserEngagementDetailDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public int LessonsCompleted { get; set; }
    public int CurrentStreak { get; set; }
    public DateTime? LastActivity { get; set; }
    public double EngagementScore { get; set; }
}

public class CourseAnalyticsDto
{
    public string CourseId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Language { get; set; } = null!;
    public int TotalEnrollments { get; set; }
    public int CompletedEnrollments { get; set; }
    public double CompletionRate { get; set; }
    public double AverageCompletionTime { get; set; }
    public List<LessonAnalyticsDto> LessonAnalytics { get; set; } = new();
    public List<CourseProgressDto> ProgressDistribution { get; set; } = new();
}

public class LessonAnalyticsDto
{
    public string LessonId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public int Order { get; set; }
    public int Attempts { get; set; }
    public int Completions { get; set; }
    public double CompletionRate { get; set; }
    public double AverageAttempts { get; set; }
    public double AverageTimeToComplete { get; set; }
}

public class CourseProgressDto
{
    public string ProgressRange { get; set; } = null!; // "0-25%", "26-50%", etc.
    public int UserCount { get; set; }
    public double Percentage { get; set; }
}

public class SystemHealthDto
{
    public DateTime LastUpdated { get; set; }
    public int ActiveSessions { get; set; }
    public double AverageResponseTime { get; set; }
    public double ErrorRate { get; set; }
    public int TotalRequests { get; set; }
    public List<EndpointHealthDto> EndpointHealth { get; set; } = new();
}

public class EndpointHealthDto
{
    public string Endpoint { get; set; } = null!;
    public string Method { get; set; } = null!;
    public double AverageResponseTime { get; set; }
    public int RequestCount { get; set; }
    public int ErrorCount { get; set; }
    public double ErrorRate { get; set; }
}

public class ReportRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string ReportType { get; set; } = null!; // user_engagement, course_analytics, system_health
    public Dictionary<string, object>? Parameters { get; set; }
}