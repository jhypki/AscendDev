using ElearningPlatform.Core.DTOs;
using ElearningPlatform.Core.DTOs.Auth;
using ElearningPlatform.Core.Filters;
using ElearningPlatform.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElearningPlatform.Functions.Controllers;

[ApiController]
[ValidateModel]
[Route("api/[controller]", Name = "auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(SuccessApiResponse<AuthResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegistrationRequest request)
    {
        var authResult = await authService.RegisterAsync(request);
        return Created("api/auth/register", ApiResponse<AuthResult>.SuccessResponse(authResult));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(SuccessApiResponse<AuthResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var authResult = await authService.LoginAsync(request);
        return Ok(ApiResponse<AuthResult>.SuccessResponse(authResult));
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(SuccessApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest refreshTokenRequest)
    {
        var token = await authService.RefreshTokenAsync(refreshTokenRequest.RefreshToken);
        return Ok(ApiResponse<string>.SuccessResponse(token));
    }

    [HttpPost("revoke-token")]
    [ProducesResponseType(typeof(SuccessApiResponse<string>), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest revokeTokenRequest)
    {
        await authService.RevokeRefreshTokenAsync(revokeTokenRequest.RefreshToken);
        return Ok(ApiResponse<string>.SuccessResponse(null!, "Token revoked successfully."));
    }
}