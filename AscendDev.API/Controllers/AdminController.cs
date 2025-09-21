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

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
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
    }
}