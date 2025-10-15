using AscendDev.Core.DTOs;
using AscendDev.Core.DTOs.Dashboard;
using AscendDev.Core.Filters;
using AscendDev.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AscendDev.API.Controllers;

[Route("[controller]")]
[ApiController]
[ValidateModel]
[Authorize]
public class DashboardController(IUserProgressService userProgressService) : ControllerBase
{
    /// <summary>
    /// Get dashboard statistics for the current user
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(DashboardStatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDashboardStats()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user ID");
        }

        var stats = await userProgressService.GetUserDashboardStatsAsync(userId);
        return Ok(stats);
    }

    /// <summary>
    /// Get user progress for all enrolled courses
    /// </summary>
    [HttpGet("progress")]
    [ProducesResponseType(typeof(List<UserProgressResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserProgress()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user ID");
        }

        var progress = await userProgressService.GetUserCoursesProgressAsync(userId);
        return Ok(progress);
    }

    /// <summary>
    /// Get learning streak data for the current user
    /// </summary>
    [HttpGet("streak")]
    [ProducesResponseType(typeof(List<LearningStreakResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLearningStreak([FromQuery] int days = 30)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user ID");
        }

        var streak = await userProgressService.GetUserLearningStreakAsync(userId, days);
        return Ok(streak);
    }

    /// <summary>
    /// Get recent activity for the current user
    /// </summary>
    [HttpGet("activity")]
    [ProducesResponseType(typeof(List<RecentActivityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRecentActivity([FromQuery] int limit = 10)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user ID");
        }

        var activity = await userProgressService.GetUserRecentActivityAsync(userId, limit);
        return Ok(activity);
    }
}