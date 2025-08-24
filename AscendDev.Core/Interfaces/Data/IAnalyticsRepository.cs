using AscendDev.Core.DTOs.Admin;
using AscendDev.Core.Models.Admin;

namespace AscendDev.Core.Interfaces.Data;

public interface IAnalyticsRepository
{
    // Dashboard Statistics
    Task<DashboardStatisticsDto> GetDashboardStatisticsAsync();
    Task<bool> UpdateDashboardStatisticAsync(string key, object value, DateTime? expiresAt = null);
    Task<DashboardStatistic?> GetDashboardStatisticAsync(string key);
    Task<bool> RefreshDashboardStatisticsAsync();

    // User Analytics
    Task<UserEngagementReportDto> GetUserEngagementReportAsync(DateTime startDate, DateTime endDate);
    Task<List<UserActivityTrendDto>> GetUserActivityTrendAsync(DateTime startDate, DateTime endDate);
    Task<int> GetActiveUsersCountAsync(DateTime startDate, DateTime endDate);
    Task<int> GetNewRegistrationsCountAsync(DateTime startDate, DateTime endDate);

    // Course Analytics
    Task<List<CourseAnalyticsDto>> GetCourseAnalyticsAsync(DateTime startDate, DateTime endDate);
    Task<CourseAnalyticsDto?> GetCourseAnalyticsAsync(string courseId, DateTime startDate, DateTime endDate);
    Task<List<PopularCourseDto>> GetPopularCoursesAsync(int limit = 10);
    Task<List<LessonCompletionTrendDto>> GetLessonCompletionTrendAsync(DateTime startDate, DateTime endDate);

    // System Health (basic metrics without performance monitoring)
    Task<SystemHealthDto> GetSystemHealthAsync();
    Task<int> GetActiveSessionsCountAsync();
    Task<List<EndpointHealthDto>> GetEndpointHealthAsync(DateTime startDate, DateTime endDate);

    // Reporting
    Task<byte[]> GenerateReportAsync(ReportRequest request);
    Task<List<Dictionary<string, object>>> GetCustomReportDataAsync(string query, Dictionary<string, object>? parameters = null);
}