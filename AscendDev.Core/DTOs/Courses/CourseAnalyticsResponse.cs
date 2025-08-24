namespace AscendDev.Core.DTOs.Courses;

public class CourseAnalyticsResponse
{
    public string CourseId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public int TotalEnrollments { get; set; }
    public int ActiveStudents { get; set; }
    public int CompletedStudents { get; set; }
    public double CompletionRate { get; set; }
    public double AverageProgress { get; set; }
    public int TotalLessons { get; set; }
    public int TotalViews { get; set; }
    public DateTime LastAccessed { get; set; }
    public List<LessonAnalytics> LessonAnalytics { get; set; } = [];
    public Dictionary<string, int> TagEngagement { get; set; } = [];
    public List<DailyEngagement> DailyEngagement { get; set; } = [];
}

public class LessonAnalytics
{
    public string LessonId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public int Views { get; set; }
    public int Completions { get; set; }
    public double CompletionRate { get; set; }
    public double AverageTimeSpent { get; set; }
    public int DropoffCount { get; set; }
}

public class DailyEngagement
{
    public DateTime Date { get; set; }
    public int Views { get; set; }
    public int UniqueUsers { get; set; }
    public int Completions { get; set; }
    public double AverageSessionTime { get; set; }
}