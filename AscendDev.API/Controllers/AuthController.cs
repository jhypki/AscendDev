using AscendDev.Core.DTOs;
using AscendDev.Core.DTOs.Auth;
using AscendDev.Core.Filters;
using AscendDev.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace AscendDev.Functions.Controllers;

[ApiController]
[ValidateModel]
[Route("api/[controller]", Name = "auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegistrationRequest request)
    {
        var authResult = await authService.RegisterAsync(request);
        return Created("api/auth/register", authResult);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var authResult = await authService.LoginAsync(request);
        return Ok(authResult);
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest refreshTokenRequest)
    {
        var authResult = await authService.RefreshTokenAsync(refreshTokenRequest.RefreshToken);
        return Ok(authResult);
    }

    [HttpPost("revoke-token")]
    [ProducesResponseType(typeof(string), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest revokeTokenRequest)
    {
        await authService.RevokeRefreshTokenAsync(revokeTokenRequest.RefreshToken);
        return Ok("Token revoked successfully.");
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest? request = null)
    {
        await authService.LogoutAsync(request?.RefreshToken);
        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Get current user information from JWT token
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult> GetCurrentUser()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var user = await authService.GetUserWithRolesByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(new { isSuccess = true, user });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving user information", error = ex.Message });
        }
    }
}