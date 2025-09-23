using AscendDev.Core.DTOs.Admin;

namespace AscendDev.Core.Interfaces.Services
{
    public interface IAdminService
    {
        Task<AdminStatsResponse> GetAdminStatsAsync();
        Task<PaginatedUserManagementResponse> GetUsersAsync(UserManagementQueryRequest request);
        Task<List<CourseAnalyticsResponse>> GetCourseAnalyticsAsync();
        Task<SystemAnalyticsResponse> GetSystemAnalyticsAsync();
        Task<bool> UpdateUserStatusAsync(string userId, bool isActive);
        Task<bool> UpdateUserRolesAsync(string userId, List<string> roles);
        Task<ReportGenerationResponse> GenerateReportAsync(GenerateReportRequest request);
    }
}