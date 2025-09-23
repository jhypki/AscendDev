using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.DTOs.UserProfile;
using AscendDev.Core.DTOs;
using System.Security.Claims;

namespace AscendDev.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserProfileController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;
    private readonly ILogger<UserProfileController> _logger;

    public UserProfileController(
        IUserProfileService userProfileService,
        ILogger<UserProfileController> logger)
    {
        _userProfileService = userProfileService;
        _logger = logger;
    }

    /// <summary>
    /// Get current user's profile
    /// </summary>
    /// <returns>Current user's profile</returns>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserProfileResponse>>> GetMyProfile()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new ErrorApiResponse(null, "Invalid user ID in token"));
            }

            var profile = await _userProfileService.GetUserProfileAsync(userId, userId);
            if (profile == null)
            {
                return NotFound(new ErrorApiResponse(null, "Profile not found"));
            }

            return Ok(new ApiResponse<UserProfileResponse>(true, profile, "Profile retrieved successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user profile");
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while retrieving profile"));
        }
    }

    /// <summary>
    /// Get user profile by ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User profile</returns>
    [HttpGet("{userId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserProfileResponse>>> GetUserProfile(Guid userId)
    {
        try
        {
            var requestingUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? requestingUserId = null;
            if (Guid.TryParse(requestingUserIdClaim, out var parsedUserId))
            {
                requestingUserId = parsedUserId;
            }

            var profile = await _userProfileService.GetUserProfileAsync(userId, requestingUserId);
            if (profile == null)
            {
                return NotFound(new ErrorApiResponse(null, "Profile not found"));
            }

            return Ok(new ApiResponse<UserProfileResponse>(true, profile, "Profile retrieved successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile {UserId}", userId);
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while retrieving profile"));
        }
    }

    /// <summary>
    /// Get user profile by username
    /// </summary>
    /// <param name="username">Username</param>
    /// <returns>User profile</returns>
    [HttpGet("username/{username}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserProfileResponse>>> GetUserProfileByUsername(string username)
    {
        try
        {
            var requestingUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? requestingUserId = null;
            if (Guid.TryParse(requestingUserIdClaim, out var parsedUserId))
            {
                requestingUserId = parsedUserId;
            }

            var profile = await _userProfileService.GetUserProfileByUsernameAsync(username, requestingUserId);
            if (profile == null)
            {
                return NotFound(new ErrorApiResponse(null, "Profile not found"));
            }

            return Ok(new ApiResponse<UserProfileResponse>(true, profile, "Profile retrieved successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile by username {Username}", username);
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while retrieving profile"));
        }
    }

    /// <summary>
    /// Update current user's profile
    /// </summary>
    /// <param name="request">Update profile request</param>
    /// <returns>Updated profile</returns>
    [HttpPut("me")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<UserProfileResponse>>> UpdateMyProfile([FromBody] UpdateUserProfileRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new ErrorApiResponse(null, "Invalid user ID in token"));
            }

            var updatedProfile = await _userProfileService.UpdateUserProfileAsync(userId, request);
            return Ok(new ApiResponse<UserProfileResponse>(true, updatedProfile, "Profile updated successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while updating profile"));
        }
    }

    /// <summary>
    /// Get user activity feed
    /// </summary>
    /// <param name="request">Activity feed request</param>
    /// <returns>Activity feed</returns>
    [HttpPost("activity")]
    [ProducesResponseType(typeof(ApiResponse<UserActivityFeedResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<UserActivityFeedResponse>>> GetUserActivityFeed([FromBody] UserActivityFeedRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var requestingUserId))
            {
                return Unauthorized(new ErrorApiResponse(null, "Invalid user ID in token"));
            }

            var activityFeed = await _userProfileService.GetUserActivityFeedAsync(request, requestingUserId);
            return Ok(new ApiResponse<UserActivityFeedResponse>(true, activityFeed, "Activity feed retrieved successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user activity feed");
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while retrieving activity feed"));
        }
    }

    /// <summary>
    /// Search user profiles
    /// </summary>
    /// <param name="request">Search request</param>
    /// <returns>Search results</returns>
    [HttpPost("search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<UserProfileSearchResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserProfileSearchResponse>>> SearchUserProfiles([FromBody] UserProfileSearchRequest request)
    {
        try
        {
            var searchResults = await _userProfileService.SearchUserProfilesAsync(request);
            return Ok(new ApiResponse<UserProfileSearchResponse>(true, searchResults, "Search completed successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching user profiles");
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while searching profiles"));
        }
    }

    /// <summary>
    /// Get user achievements
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User achievements</returns>
    [HttpGet("{userId:guid}/achievements")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<UserAchievementResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<UserAchievementResponse>>>> GetUserAchievements(Guid userId)
    {
        try
        {
            var achievements = await _userProfileService.GetUserAchievementsAsync(userId);
            return Ok(new ApiResponse<List<UserAchievementResponse>>(true, achievements, "Achievements retrieved successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user achievements for {UserId}", userId);
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while retrieving achievements"));
        }
    }

    /// <summary>
    /// Get user statistics
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User statistics</returns>
    [HttpGet("{userId:guid}/statistics")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<UserProfileStatistics>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserProfileStatistics>>> GetUserStatistics(Guid userId)
    {
        try
        {
            var statistics = await _userProfileService.GetUserStatisticsAsync(userId);
            return Ok(new ApiResponse<UserProfileStatistics>(true, statistics, "Statistics retrieved successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user statistics for {UserId}", userId);
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while retrieving statistics"));
        }
    }
}