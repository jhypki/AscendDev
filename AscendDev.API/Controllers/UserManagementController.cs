using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.DTOs.Admin;
using AscendDev.Core.DTOs;
using System.Security.Claims;

namespace AscendDev.API.Controllers;

[ApiController]
[Route("admin/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class UserManagementController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;
    private readonly ILogger<UserManagementController> _logger;

    public UserManagementController(
        IUserManagementService userManagementService,
        ILogger<UserManagementController> logger)
    {
        _userManagementService = userManagementService;
        _logger = logger;
    }

    /// <summary>
    /// Search and filter users with pagination
    /// </summary>
    [HttpPost("search")]
    public async Task<ActionResult<ApiResponse<PagedResult<UserManagementDto>>>> SearchUsers(
        [FromBody] UserSearchRequest request)
    {
        try
        {
            var result = await _userManagementService.SearchUsersAsync(request);
            return Ok(new ApiResponse<PagedResult<UserManagementDto>>(true, result, "Users retrieved successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users");
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while searching users"));
        }
    }

    /// <summary>
    /// Get detailed user information by ID
    /// </summary>
    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<ApiResponse<UserManagementDto>>> GetUser(Guid userId)
    {
        try
        {
            var user = await _userManagementService.GetUserWithDetailsAsync(userId);
            if (user == null)
            {
                return NotFound(new ErrorApiResponse(null, "User not found"));
            }

            return Ok(new ApiResponse<UserManagementDto>(true, user, "User retrieved successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", userId);
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while retrieving user"));
        }
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserManagementDto>>> CreateUser(
        [FromBody] CreateUserRequest request)
    {
        try
        {
            var user = await _userManagementService.CreateUserAsync(request);

            // Log admin activity
            var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            await _userManagementService.LogUserActivityAsync(
                adminId,
                "user_created",
                $"Created user: {user.Username}",
                new Dictionary<string, object> { { "created_user_id", user.Id } },
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return CreatedAtAction(nameof(GetUser), new { userId = user.Id },
                new ApiResponse<UserManagementDto>(true, user, "User created successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while creating user"));
        }
    }

    /// <summary>
    /// Update user information
    /// </summary>
    [HttpPut("{userId:guid}")]
    public async Task<ActionResult<ApiResponse<UserManagementDto>>> UpdateUser(
        Guid userId, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var user = await _userManagementService.UpdateUserAsync(userId, request);

            // Log admin activity
            var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            await _userManagementService.LogUserActivityAsync(
                adminId,
                "user_updated",
                $"Updated user: {user.Username}",
                new Dictionary<string, object> { { "updated_user_id", userId } },
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return Ok(new ApiResponse<UserManagementDto>(true, user, "User updated successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", userId);
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while updating user"));
        }
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    [HttpDelete("{userId:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(Guid userId)
    {
        try
        {
            var result = await _userManagementService.DeleteUserAsync(userId);
            if (!result)
            {
                return NotFound(new ErrorApiResponse(null, "User not found"));
            }

            // Log admin activity
            var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            await _userManagementService.LogUserActivityAsync(
                adminId,
                "user_deleted",
                $"Deleted user with ID: {userId}",
                new Dictionary<string, object> { { "deleted_user_id", userId } },
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return Ok(new ApiResponse<bool>(true, result, "User deleted successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while deleting user"));
        }
    }

    /// <summary>
    /// Activate a user account
    /// </summary>
    [HttpPost("{userId:guid}/activate")]
    public async Task<ActionResult<ApiResponse<bool>>> ActivateUser(Guid userId)
    {
        try
        {
            var result = await _userManagementService.ActivateUserAsync(userId);

            // Log admin activity
            var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            await _userManagementService.LogUserActivityAsync(
                adminId,
                "user_activated",
                $"Activated user with ID: {userId}",
                new Dictionary<string, object> { { "activated_user_id", userId } },
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return Ok(new ApiResponse<bool>(true, result, "User activated successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating user {UserId}", userId);
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while activating user"));
        }
    }

    /// <summary>
    /// Deactivate a user account
    /// </summary>
    [HttpPost("{userId:guid}/deactivate")]
    public async Task<ActionResult<ApiResponse<bool>>> DeactivateUser(Guid userId)
    {
        try
        {
            var result = await _userManagementService.DeactivateUserAsync(userId);

            // Log admin activity
            var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            await _userManagementService.LogUserActivityAsync(
                adminId,
                "user_deactivated",
                $"Deactivated user with ID: {userId}",
                new Dictionary<string, object> { { "deactivated_user_id", userId } },
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return Ok(new ApiResponse<bool>(true, result, "User deactivated successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user {UserId}", userId);
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while deactivating user"));
        }
    }

    /// <summary>
    /// Lock a user account
    /// </summary>
    [HttpPost("{userId:guid}/lock")]
    public async Task<ActionResult<ApiResponse<bool>>> LockUser(Guid userId, [FromQuery] DateTime? lockUntil = null)
    {
        try
        {
            var result = await _userManagementService.LockUserAsync(userId, lockUntil);

            // Log admin activity
            var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            await _userManagementService.LogUserActivityAsync(
                adminId,
                "user_locked",
                $"Locked user with ID: {userId}" + (lockUntil.HasValue ? $" until {lockUntil}" : ""),
                new Dictionary<string, object> { { "locked_user_id", userId }, { "lock_until", lockUntil } },
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return Ok(new ApiResponse<bool>(true, result, "User locked successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error locking user {UserId}", userId);
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while locking user"));
        }
    }

    /// <summary>
    /// Unlock a user account
    /// </summary>
    [HttpPost("{userId:guid}/unlock")]
    public async Task<ActionResult<ApiResponse<bool>>> UnlockUser(Guid userId)
    {
        try
        {
            var result = await _userManagementService.UnlockUserAsync(userId);

            // Log admin activity
            var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            await _userManagementService.LogUserActivityAsync(
                adminId,
                "user_unlocked",
                $"Unlocked user with ID: {userId}",
                new Dictionary<string, object> { { "unlocked_user_id", userId } },
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return Ok(new ApiResponse<bool>(true, result, "User unlocked successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking user {UserId}", userId);
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while unlocking user"));
        }
    }

    /// <summary>
    /// Get user activity logs
    /// </summary>
    [HttpGet("{userId:guid}/activity")]
    public async Task<ActionResult<ApiResponse<List<UserActivityLogDto>>>> GetUserActivity(
        Guid userId, [FromQuery] int limit = 100)
    {
        try
        {
            var activities = await _userManagementService.GetUserActivityAsync(userId, limit);
            return Ok(new ApiResponse<List<UserActivityLogDto>>(true, activities, "User activity retrieved successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user activity for {UserId}", userId);
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while retrieving user activity"));
        }
    }

    /// <summary>
    /// Get recent system activity
    /// </summary>
    [HttpGet("activity/recent")]
    public async Task<ActionResult<ApiResponse<List<UserActivityLogDto>>>> GetRecentActivity(
        [FromQuery] int limit = 100)
    {
        try
        {
            var activities = await _userManagementService.GetRecentActivityAsync(limit);
            return Ok(new ApiResponse<List<UserActivityLogDto>>(true, activities, "Recent activity retrieved successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent activity");
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while retrieving recent activity"));
        }
    }

    /// <summary>
    /// Perform bulk operations on users
    /// </summary>
    [HttpPost("bulk-operation")]
    public async Task<ActionResult<ApiResponse<BulkOperationResult>>> PerformBulkOperation(
        [FromBody] BulkUserOperationRequest request)
    {
        try
        {
            var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var result = await _userManagementService.PerformBulkOperationAsync(request, adminId);

            return Ok(new ApiResponse<BulkOperationResult>(true, result, "Bulk operation completed successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk operation");
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while performing bulk operation"));
        }
    }

    /// <summary>
    /// Get bulk operation result
    /// </summary>
    [HttpGet("bulk-operation/{operationId:guid}")]
    public async Task<ActionResult<ApiResponse<BulkOperationResult>>> GetBulkOperationResult(Guid operationId)
    {
        try
        {
            var result = await _userManagementService.GetBulkOperationResultAsync(operationId);
            if (result == null)
            {
                return NotFound(new ErrorApiResponse(null, "Bulk operation not found"));
            }

            return Ok(new ApiResponse<BulkOperationResult>(true, result, "Bulk operation result retrieved successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bulk operation result {OperationId}", operationId);
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while retrieving bulk operation result"));
        }
    }
}