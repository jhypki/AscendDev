namespace AscendDev.Core.DTOs.Admin
{
    public class AdminStatsResponse
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewRegistrations { get; set; }
        public int TotalCourses { get; set; }
        public int PublishedCourses { get; set; }
        public int TotalLessons { get; set; }
        public string SystemHealth { get; set; } = "healthy";
        public double ServerUptime { get; set; }
    }
}