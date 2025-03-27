using ElearningPlatform.Core.DTOs;
using ElearningPlatform.Core.DTOs.Auth;
using ElearningPlatform.Core.Exceptions;
using ElearningPlatform.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElearningPlatform.Functions.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(SuccessApiResponse<AuthResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegistrationRequest request)
    {
        if (!ModelState.IsValid) throw new ValidationException(ModelState);

        var authResult = await authService.RegisterAsync(request);
        return Ok(ApiResponse<AuthResult>.SuccessResponse(authResult));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(SuccessApiResponse<AuthResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid) throw new ValidationException(ModelState);

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
        if (!ModelState.IsValid) throw new ValidationException(ModelState);

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
        if (!ModelState.IsValid) throw new ValidationException(ModelState);

        await authService.RevokeRefreshTokenAsync(revokeTokenRequest.RefreshToken);
        return NoContent();
    }
}