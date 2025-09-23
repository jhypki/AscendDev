using AscendDev.Core.DTOs.Admin;
using AscendDev.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AscendDev.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IUserManagementService _userManagementService;

        public AdminController(IAdminService adminService, IUserManagementService userManagementService)
        {
            _adminService = adminService;
            _userManagementService = userManagementService;
        }

        /// <summary>
        /// Get admin dashboard statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<AdminStatsResponse>> GetAdminStats()
        {
            try
            {
                var stats = await _adminService.GetAdminStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving admin statistics", error = ex.Message });
            }
        }

        /// <summary>
        /// Get paginated user management data
        /// </summary>
        [HttpGet("users")]
        public async Task<ActionResult<PaginatedUserManagementResponse>> GetUsers([FromQuery] UserManagementQueryRequest request)
        {
            try
            {
                var users = await _adminService.GetUsersAsync(request);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving users", error = ex.Message });
            }
        }

        /// <summary>
        /// Get course analytics data
        /// </summary>
        [HttpGet("course-analytics")]
        public async Task<ActionResult<List<CourseAnalyticsResponse>>> GetCourseAnalytics()
        {
            try
            {
                var analytics = await _adminService.GetCourseAnalyticsAsync();
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving course analytics", error = ex.Message });
            }
        }

        /// <summary>
        /// Get system analytics data
        /// </summary>
        [HttpGet("system-analytics")]
        public async Task<ActionResult<SystemAnalyticsResponse>> GetSystemAnalytics()
        {
            try
            {
                var analytics = await _adminService.GetSystemAnalyticsAsync();
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving system analytics", error = ex.Message });
            }
        }

        /// <summary>
        /// Update user status (active/inactive)
        /// </summary>
        [HttpPut("users/{userId}/status")]
        public async Task<ActionResult> UpdateUserStatus(string userId, [FromBody] UpdateUserStatusRequest request)
        {
            try
            {
                var success = await _adminService.UpdateUserStatusAsync(userId, request.IsActive);
                if (!success)
                {
                    return NotFound(new { message = "User not found" });
                }
                return Ok(new { message = "User status updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating user status", error = ex.Message });
            }
        }

        /// <summary>
        /// Update user roles
        /// </summary>
        [HttpPut("users/{userId}/roles")]
        public async Task<ActionResult> UpdateUserRoles(string userId, [FromBody] UpdateUserRolesRequest request)
        {
            try
            {
                var success = await _adminService.UpdateUserRolesAsync(userId, request.Roles);
                if (!success)
                {
                    return NotFound(new { message = "User not found" });
                }
                return Ok(new { message = "User roles updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating user roles", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new user manually
        /// </summary>
        [HttpPost("users")]
        public async Task<ActionResult<UserManagementDto>> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                var user = await _userManagementService.CreateUserAsync(request);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating user", error = ex.Message });
            }
        }

        /// <summary>
        /// Generate system report
        /// </summary>
        [HttpPost("reports/generate")]
        public async Task<ActionResult<ReportGenerationResponse>> GenerateReport([FromBody] GenerateReportRequest request)
        {
            try
            {
                var report = await _adminService.GenerateReportAsync(request);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while generating report", error = ex.Message });
            }
        }

        /// <summary>
        /// Get available report types
        /// </summary>
        [HttpGet("reports/types")]
        public ActionResult<List<ReportTypeResponse>> GetReportTypes()
        {
            try
            {
                var reportTypes = new List<ReportTypeResponse>
                {
                    new ReportTypeResponse
                    {
                        Id = "user-activity",
                        Name = "User Activity Report",
                        Description = "Detailed report of user activities and engagement"
                    },
                    new ReportTypeResponse
                    {
                        Id = "course-analytics",
                        Name = "Course Analytics Report",
                        Description = "Comprehensive analysis of course performance and enrollment"
                    },
                    new ReportTypeResponse
                    {
                        Id = "system-health",
                        Name = "System Health Report",
                        Description = "System performance and health metrics"
                    }
                };
                return Ok(reportTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving report types", error = ex.Message });
            }
        }

        /// <summary>
        /// Get system settings (placeholder)
        /// </summary>
        [HttpGet("settings")]
        public ActionResult<SystemSettingsResponse> GetSystemSettings()
        {
            try
            {
                var settings = new SystemSettingsResponse
                {
                    MaintenanceMode = false,
                    RegistrationEnabled = true,
                    EmailNotificationsEnabled = true,
                    MaxUsersPerCourse = 1000,
                    SessionTimeoutMinutes = 60,
                    BackupFrequencyHours = 24
                };
                return Ok(settings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving system settings", error = ex.Message });
            }
        }

        /// <summary>
        /// Update system settings (placeholder)
        /// </summary>
        [HttpPut("settings")]
        public ActionResult UpdateSystemSettings([FromBody] UpdateSystemSettingsRequest request)
        {
            try
            {
                // Placeholder implementation - would update actual system settings
                return Ok(new { message = "System settings updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating system settings", error = ex.Message });
            }
        }
    }
}