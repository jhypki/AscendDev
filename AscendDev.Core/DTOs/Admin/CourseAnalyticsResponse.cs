namespace AscendDev.Core.DTOs.Admin
{
    public class CourseAnalyticsResponse
    {
        public string CourseId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Enrollments { get; set; }
        public int Completions { get; set; }
        public double AverageRating { get; set; }
        public int TotalLessons { get; set; }
        public string Language { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public double CompletionRate => Enrollments > 0 ? (double)Completions / Enrollments * 100 : 0;
    }

    public class SystemAnalyticsResponse
    {
        public List<UserGrowthData> UserGrowth { get; set; } = new();
        public List<TopCourseData> TopCourses { get; set; } = new();
    }

    public class UserGrowthData
    {
        public string Date { get; set; } = string.Empty;
        public int Users { get; set; }
        public int Courses { get; set; }
        public int Lessons { get; set; }
    }

    public class TopCourseData
    {
        public string Name { get; set; } = string.Empty;
        public int Enrollments { get; set; }
    }

    public class UpdateUserStatusRequest
    {
        public string UserId { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class UpdateUserRolesRequest
    {
        public string UserId { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
}