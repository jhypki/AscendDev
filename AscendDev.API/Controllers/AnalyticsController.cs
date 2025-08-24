using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.DTOs.Admin;
using AscendDev.Core.DTOs;

namespace AscendDev.API.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        IAnalyticsService analyticsService,
        ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    /// <summary>
    /// Get dashboard statistics
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<DashboardStatisticsDto>>> GetDashboardStatistics()
    {
        try
        {
            var statistics = await _analyticsService.GetDashboardStatisticsAsync();
            return Ok(new ApiResponse<DashboardStatisticsDto>(true, statistics, "Dashboard statistics retrieved successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard statistics");
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while retrieving dashboard statistics"));
        }
    }

    /// <summary>
    /// Refresh dashboard statistics cache
    /// </summary>
    [HttpPost("dashboard/refresh")]
    public async Task<ActionResult<ApiResponse<bool>>> RefreshDashboardStatistics()
    {
        try
        {
            var result = await _analyticsService.RefreshDashboardStatisticsAsync();
            return Ok(new ApiResponse<bool>(true, result, "Dashboard statistics refreshed successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing dashboard statistics");
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while refreshing dashboard statistics"));
        }
    }

    /// <summary>
    /// Get user engagement report
    /// </summary>
    [HttpGet("user-engagement")]
    public async Task<ActionResult<ApiResponse<UserEngagementReportDto>>> GetUserEngagementReport(
        [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var report = await _analyticsService.GetUserEngagementReportAsync(startDate, endDate);
            return Ok(new ApiResponse<UserEngagementReportDto>(true, report, "User engagement report retrieved successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user engagement report");
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while retrieving user engagement report"));
        }
    }

    /// <summary>
    /// Get user activity trend
    /// </summary>
    [HttpGet("user-activity-trend")]
    public async Task<ActionResult<ApiResponse<List<UserActivityTrendDto>>>> GetUserActivityTrend(
        [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var trend = await _analyticsService.GetUserActivityTrendAsync(startDate, endDate);
            return Ok(new ApiResponse<List<UserActivityTrendDto>>(true, trend, "User activity trend retrieved successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user activity trend");
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while retrieving user activity trend"));
        }
    }

    /// <summary>
    /// Get active users count
    /// </summary>
    [HttpGet("active-users")]
    public async Task<ActionResult<ApiResponse<int>>> GetActiveUsersCount(
        [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var count = await _analyticsService.GetActiveUsersCountAsync(startDate, endDate);
            return Ok(new ApiResponse<int>(true, count, "Active users count retrieved successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active users count");
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while retrieving active users count"));
        }
    }

    /// <summary>
    /// Get new registrations count
    /// </summary>
    [HttpGet("new-registrations")]
    public async Task<ActionResult<ApiResponse<int>>> GetNewRegistrationsCount(
        [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var count = await _analyticsService.GetNewRegistrationsCountAsync(startDate, endDate);
            return Ok(new ApiResponse<int>(true, count, "New registrations count retrieved successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting new registrations count");
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while retrieving new registrations count"));
        }
    }

    /// <summary>
    /// Get course analytics
    /// </summary>
    [HttpGet("courses")]
    public async Task<ActionResult<ApiResponse<List<CourseAnalyticsDto>>>> GetCourseAnalytics(
        [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var analytics = await _analyticsService.GetCourseAnalyticsAsync(startDate, endDate);
            return Ok(new ApiResponse<List<CourseAnalyticsDto>>(true, analytics, "Course analytics retrieved successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course analytics");
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while retrieving course analytics"));
        }
    }

    /// <summary>
    /// Get specific course analytics
    /// </summary>
    [HttpGet("courses/{courseId}")]
    public async Task<ActionResult<ApiResponse<CourseAnalyticsDto>>> GetCourseAnalytics(
        string courseId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var analytics = await _analyticsService.GetCourseAnalyticsAsync(courseId, startDate, endDate);
            if (analytics == null)
            {
                return NotFound(new ErrorApiResponse(null, "Course analytics not found"));
            }

            return Ok(new ApiResponse<CourseAnalyticsDto>(true, analytics, "Course analytics retrieved successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course analytics for {CourseId}", courseId);
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while retrieving course analytics"));
        }
    }

    /// <summary>
    /// Get popular courses
    /// </summary>
    [HttpGet("popular-courses")]
    public async Task<ActionResult<ApiResponse<List<PopularCourseDto>>>> GetPopularCourses(
        [FromQuery] int limit = 10)
    {
        try
        {
            var courses = await _analyticsService.GetPopularCoursesAsync(limit);
            return Ok(new ApiResponse<List<PopularCourseDto>>(true, courses, "Popular courses retrieved successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting popular courses");
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while retrieving popular courses"));
        }
    }

    /// <summary>
    /// Get lesson completion trend
    /// </summary>
    [HttpGet("lesson-completion-trend")]
    public async Task<ActionResult<ApiResponse<List<LessonCompletionTrendDto>>>> GetLessonCompletionTrend(
        [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var trend = await _analyticsService.GetLessonCompletionTrendAsync(startDate, endDate);
            return Ok(new ApiResponse<List<LessonCompletionTrendDto>>(true, trend, "Lesson completion trend retrieved successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lesson completion trend");
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while retrieving lesson completion trend"));
        }
    }

    /// <summary>
    /// Get system health
    /// </summary>
    [HttpGet("system-health")]
    public async Task<ActionResult<ApiResponse<SystemHealthDto>>> GetSystemHealth()
    {
        try
        {
            var health = await _analyticsService.GetSystemHealthAsync();
            return Ok(new ApiResponse<SystemHealthDto>(true, health, "System health retrieved successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system health");
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while retrieving system health"));
        }
    }

    /// <summary>
    /// Get active sessions count
    /// </summary>
    [HttpGet("active-sessions")]
    public async Task<ActionResult<ApiResponse<int>>> GetActiveSessionsCount()
    {
        try
        {
            var count = await _analyticsService.GetActiveSessionsCountAsync();
            return Ok(new ApiResponse<int>(true, count, "Active sessions count retrieved successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active sessions count");
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while retrieving active sessions count"));
        }
    }

    /// <summary>
    /// Generate custom report
    /// </summary>
    [HttpPost("reports/generate")]
    public async Task<ActionResult> GenerateReport([FromBody] ReportRequest request)
    {
        try
        {
            var reportData = await _analyticsService.GenerateReportAsync(request);

            var contentType = request.ReportType switch
            {
                "pdf" => "application/pdf",
                "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "csv" => "text/csv",
                _ => "application/octet-stream"
            };

            var fileName = $"report_{request.ReportType}_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
            var fileExtension = request.ReportType switch
            {
                "pdf" => ".pdf",
                "excel" => ".xlsx",
                "csv" => ".csv",
                _ => ".bin"
            };

            return File(reportData, contentType, fileName + fileExtension);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating report");
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while generating report"));
        }
    }

    /// <summary>
    /// Execute custom query for reporting
    /// </summary>
    [HttpPost("reports/custom-query")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<List<Dictionary<string, object>>>>> ExecuteCustomQuery(
        [FromBody] CustomQueryRequest request)
    {
        try
        {
            var data = await _analyticsService.GetCustomReportDataAsync(request.Query, request.Parameters);
            return Ok(new ApiResponse<List<Dictionary<string, object>>>(true, data, "Custom query executed successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing custom query");
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while executing custom query"));
        }
    }
}

public class CustomQueryRequest
{
    public string Query { get; set; } = null!;
    public Dictionary<string, object>? Parameters { get; set; }
}