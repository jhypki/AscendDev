using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.DTOs.Admin;
using AscendDev.Core.DTOs;
using AscendDev.Core.Models.Auth;
using System.Security.Claims;

namespace AscendDev.API.Controllers;

[ApiController]
[Route("admin/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class RoleManagementController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;
    private readonly ILogger<RoleManagementController> _logger;

    public RoleManagementController(
        IUserManagementService userManagementService,
        ILogger<RoleManagementController> logger)
    {
        _userManagementService = userManagementService;
        _logger = logger;
    }

    /// <summary>
    /// Get all available roles
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<Role>>>> GetAllRoles()
    {
        try
        {
            var roles = await _userManagementService.GetAllRolesAsync();
            return Ok(new ApiResponse<List<Role>>(true, roles, "Roles retrieved successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all roles");
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while retrieving roles"));
        }
    }

    /// <summary>
    /// Get user roles by user ID
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetUserRoles(Guid userId)
    {
        try
        {
            var roles = await _userManagementService.GetUserRolesAsync(userId);
            return Ok(new ApiResponse<List<string>>(true, roles, "User roles retrieved successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user roles for {UserId}", userId);
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while retrieving user roles"));
        }
    }

    /// <summary>
    /// Assign role to user
    /// </summary>
    [HttpPost("user/{userId:guid}/assign")]
    public async Task<ActionResult<ApiResponse<bool>>> AssignRoleToUser(
        Guid userId, [FromBody] AssignRoleRequest request)
    {
        try
        {
            var result = await _userManagementService.AssignRoleToUserAsync(userId, request.RoleName);

            // Log admin activity
            var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            await _userManagementService.LogUserActivityAsync(
                adminId,
                "role_assigned",
                $"Assigned role '{request.RoleName}' to user {userId}",
                new Dictionary<string, object> { { "user_id", userId }, { "role_name", request.RoleName } },
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return Ok(new ApiResponse<bool>(true, result, "Role assigned successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role {RoleName} to user {UserId}", request.RoleName, userId);
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while assigning role"));
        }
    }

    /// <summary>
    /// Remove role from user
    /// </summary>
    [HttpPost("user/{userId:guid}/remove")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveRoleFromUser(
        Guid userId, [FromBody] RemoveRoleRequest request)
    {
        try
        {
            var result = await _userManagementService.RemoveRoleFromUserAsync(userId, request.RoleName);

            // Log admin activity
            var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            await _userManagementService.LogUserActivityAsync(
                adminId,
                "role_removed",
                $"Removed role '{request.RoleName}' from user {userId}",
                new Dictionary<string, object> { { "user_id", userId }, { "role_name", request.RoleName } },
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return Ok(new ApiResponse<bool>(true, result, "Role removed successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role {RoleName} from user {UserId}", request.RoleName, userId);
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while removing role"));
        }
    }

    /// <summary>
    /// Update user roles (replace all existing roles)
    /// </summary>
    [HttpPut("user/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateUserRoles(
        Guid userId, [FromBody] UpdateUserRolesRequest request)
    {
        try
        {
            var result = await _userManagementService.UpdateUserRolesAsync(userId, request.Roles);

            // Log admin activity
            var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            await _userManagementService.LogUserActivityAsync(
                adminId,
                "roles_updated",
                $"Updated roles for user {userId}",
                new Dictionary<string, object> { { "user_id", userId }, { "new_roles", request.Roles } },
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return Ok(new ApiResponse<bool>(true, result, "User roles updated successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating roles for user {UserId}", userId);
            return StatusCode(500, new ErrorApiResponse(null, "An error occurred while updating user roles"));
        }
    }
}

public class AssignRoleRequest
{
    public string RoleName { get; set; } = null!;
}

public class RemoveRoleRequest
{
    public string RoleName { get; set; } = null!;
}

public class UpdateUserRolesRequest
{
    public List<string> Roles { get; set; } = new();
}