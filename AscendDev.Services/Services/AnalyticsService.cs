using AscendDev.Core.DTOs.Admin;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace AscendDev.Services.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsRepository _analyticsRepository;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(
        IAnalyticsRepository analyticsRepository,
        ILogger<AnalyticsService> logger)
    {
        _analyticsRepository = analyticsRepository;
        _logger = logger;
    }

    public async Task<DashboardStatisticsDto> GetDashboardStatisticsAsync()
    {
        try
        {
            return await _analyticsRepository.GetDashboardStatisticsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard statistics");
            throw new InternalServerErrorException("An error occurred while retrieving dashboard statistics");
        }
    }

    public async Task<bool> RefreshDashboardStatisticsAsync()
    {
        try
        {
            return await _analyticsRepository.RefreshDashboardStatisticsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing dashboard statistics");
            throw new InternalServerErrorException("An error occurred while refreshing dashboard statistics");
        }
    }

    public async Task<UserEngagementReportDto> GetUserEngagementReportAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            if (startDate > endDate)
            {
                throw new BadRequestException("Start date cannot be after end date");
            }

            return await _analyticsRepository.GetUserEngagementReportAsync(startDate, endDate);
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user engagement report for period {StartDate} to {EndDate}", startDate, endDate);
            throw new InternalServerErrorException("An error occurred while retrieving user engagement report");
        }
    }

    public async Task<List<UserActivityTrendDto>> GetUserActivityTrendAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            if (startDate > endDate)
            {
                throw new BadRequestException("Start date cannot be after end date");
            }

            return await _analyticsRepository.GetUserActivityTrendAsync(startDate, endDate);
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user activity trend for period {StartDate} to {EndDate}", startDate, endDate);
            throw new InternalServerErrorException("An error occurred while retrieving user activity trend");
        }
    }

    public async Task<int> GetActiveUsersCountAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            if (startDate > endDate)
            {
                throw new BadRequestException("Start date cannot be after end date");
            }

            return await _analyticsRepository.GetActiveUsersCountAsync(startDate, endDate);
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active users count for period {StartDate} to {EndDate}", startDate, endDate);
            throw new InternalServerErrorException("An error occurred while retrieving active users count");
        }
    }

    public async Task<int> GetNewRegistrationsCountAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            if (startDate > endDate)
            {
                throw new BadRequestException("Start date cannot be after end date");
            }

            return await _analyticsRepository.GetNewRegistrationsCountAsync(startDate, endDate);
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting new registrations count for period {StartDate} to {EndDate}", startDate, endDate);
            throw new InternalServerErrorException("An error occurred while retrieving new registrations count");
        }
    }

    public async Task<List<CourseAnalyticsDto>> GetCourseAnalyticsAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            if (startDate > endDate)
            {
                throw new BadRequestException("Start date cannot be after end date");
            }

            return await _analyticsRepository.GetCourseAnalyticsAsync(startDate, endDate);
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course analytics for period {StartDate} to {EndDate}", startDate, endDate);
            throw new InternalServerErrorException("An error occurred while retrieving course analytics");
        }
    }

    public async Task<CourseAnalyticsDto?> GetCourseAnalyticsAsync(string courseId, DateTime startDate, DateTime endDate)
    {
        try
        {
            if (string.IsNullOrEmpty(courseId))
            {
                throw new BadRequestException("Course ID cannot be empty");
            }

            if (startDate > endDate)
            {
                throw new BadRequestException("Start date cannot be after end date");
            }

            var result = await _analyticsRepository.GetCourseAnalyticsAsync(courseId, startDate, endDate);
            if (result == null)
            {
                throw new NotFoundException("Course not found or no analytics data available");
            }

            return result;
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course analytics for course {CourseId} and period {StartDate} to {EndDate}", courseId, startDate, endDate);
            throw new InternalServerErrorException("An error occurred while retrieving course analytics");
        }
    }

    public async Task<List<PopularCourseDto>> GetPopularCoursesAsync(int limit = 10)
    {
        try
        {
            if (limit <= 0 || limit > 100)
            {
                throw new BadRequestException("Limit must be between 1 and 100");
            }

            return await _analyticsRepository.GetPopularCoursesAsync(limit);
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting popular courses with limit {Limit}", limit);
            throw new InternalServerErrorException("An error occurred while retrieving popular courses");
        }
    }

    public async Task<List<LessonCompletionTrendDto>> GetLessonCompletionTrendAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            if (startDate > endDate)
            {
                throw new BadRequestException("Start date cannot be after end date");
            }

            return await _analyticsRepository.GetLessonCompletionTrendAsync(startDate, endDate);
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lesson completion trend for period {StartDate} to {EndDate}", startDate, endDate);
            throw new InternalServerErrorException("An error occurred while retrieving lesson completion trend");
        }
    }

    public async Task<SystemHealthDto> GetSystemHealthAsync()
    {
        try
        {
            return await _analyticsRepository.GetSystemHealthAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system health");
            throw new InternalServerErrorException("An error occurred while retrieving system health");
        }
    }

    public async Task<int> GetActiveSessionsCountAsync()
    {
        try
        {
            return await _analyticsRepository.GetActiveSessionsCountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active sessions count");
            throw new InternalServerErrorException("An error occurred while retrieving active sessions count");
        }
    }

    public async Task<List<EndpointHealthDto>> GetEndpointHealthAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            if (startDate > endDate)
            {
                throw new BadRequestException("Start date cannot be after end date");
            }

            return await _analyticsRepository.GetEndpointHealthAsync(startDate, endDate);
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting endpoint health for period {StartDate} to {EndDate}", startDate, endDate);
            throw new InternalServerErrorException("An error occurred while retrieving endpoint health");
        }
    }

    public async Task<byte[]> GenerateReportAsync(ReportRequest request)
    {
        try
        {
            if (request.StartDate > request.EndDate)
            {
                throw new BadRequestException("Start date cannot be after end date");
            }

            if (string.IsNullOrEmpty(request.ReportType))
            {
                throw new BadRequestException("Report type is required");
            }

            var validReportTypes = new[] { "user_engagement", "course_analytics", "system_health", "pdf", "excel", "csv" };
            if (!validReportTypes.Contains(request.ReportType.ToLower()))
            {
                throw new BadRequestException($"Invalid report type. Valid types are: {string.Join(", ", validReportTypes)}");
            }

            return await _analyticsRepository.GenerateReportAsync(request);
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating report of type {ReportType} for period {StartDate} to {EndDate}",
                request.ReportType, request.StartDate, request.EndDate);
            throw new InternalServerErrorException("An error occurred while generating report");
        }
    }

    public async Task<List<Dictionary<string, object>>> GetCustomReportDataAsync(string query, Dictionary<string, object>? parameters = null)
    {
        try
        {
            if (string.IsNullOrEmpty(query))
            {
                throw new BadRequestException("Query cannot be empty");
            }

            // Basic SQL injection protection - only allow SELECT statements
            var trimmedQuery = query.Trim().ToUpper();
            if (!trimmedQuery.StartsWith("SELECT"))
            {
                throw new BadRequestException("Only SELECT queries are allowed");
            }

            // Prevent dangerous operations
            var dangerousKeywords = new[] { "DROP", "DELETE", "UPDATE", "INSERT", "ALTER", "CREATE", "TRUNCATE", "EXEC", "EXECUTE" };
            if (dangerousKeywords.Any(keyword => trimmedQuery.Contains(keyword)))
            {
                throw new BadRequestException("Query contains prohibited operations");
            }

            return await _analyticsRepository.GetCustomReportDataAsync(query, parameters);
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing custom query: {Query}", query);
            throw new InternalServerErrorException("An error occurred while executing custom query");
        }
    }
}