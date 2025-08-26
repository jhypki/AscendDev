using AscendDev.Core.DTOs.Courses;
using AscendDev.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AscendDev.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserSettingsController : ControllerBase
{
    private readonly IUserSettingsService _userSettingsService;

    public UserSettingsController(IUserSettingsService userSettingsService)
    {
        _userSettingsService = userSettingsService;
    }

    /// <summary>
    /// Get current user's settings
    /// </summary>
    /// <returns>User's settings</returns>
    [HttpGet]
    public async Task<ActionResult<UserSettingsResponse>> GetUserSettings()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user ID in token");
        }

        var settings = await _userSettingsService.GetOrCreateUserSettingsAsync(userId);
        return Ok(settings);
    }

    /// <summary>
    /// Update current user's settings
    /// </summary>
    /// <param name="request">Updated settings</param>
    /// <returns>Updated user settings</returns>
    [HttpPut]
    public async Task<ActionResult<UserSettingsResponse>> UpdateUserSettings([FromBody] UpdateUserSettingsRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user ID in token");
        }

        var updatedSettings = await _userSettingsService.UpdateUserSettingsAsync(userId, request);
        return Ok(updatedSettings);
    }

    /// <summary>
    /// Check if a user has public submissions enabled
    /// </summary>
    /// <param name="userId">The user ID to check</param>
    /// <returns>Whether public submissions are enabled</returns>
    [HttpGet("{userId}/public-submissions-enabled")]
    [AllowAnonymous]
    public async Task<ActionResult<bool>> IsPublicSubmissionsEnabled(Guid userId)
    {
        var isEnabled = await _userSettingsService.IsPublicSubmissionsEnabledAsync(userId);
        return Ok(isEnabled);
    }

    /// <summary>
    /// Delete current user's settings (reset to defaults)
    /// </summary>
    /// <returns>Success status</returns>
    [HttpDelete]
    public async Task<ActionResult> DeleteUserSettings()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user ID in token");
        }

        await _userSettingsService.DeleteUserSettingsAsync(userId);
        return NoContent();
    }
}