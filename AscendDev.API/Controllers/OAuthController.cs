using AscendDev.Core.DTOs.Auth;
using AscendDev.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AscendDev.API.Controllers;

[ApiController]
[Route("auth/oauth")]
public class OAuthController : ControllerBase
{
    private readonly IOAuthService _oauthService;
    private readonly ILogger<OAuthController> _logger;

    public OAuthController(IOAuthService oauthService, ILogger<OAuthController> logger)
    {
        _oauthService = oauthService;
        _logger = logger;
    }

    /// <summary>
    /// Get OAuth authorization URL for a provider
    /// </summary>
    /// <param name="provider">OAuth provider (github, google)</param>
    /// <param name="redirectUri">Optional redirect URI</param>
    /// <returns>Authorization URL and state</returns>
    [HttpGet("{provider}/authorize")]
    public async Task<IActionResult> GetAuthorizationUrl(
        string provider,
        [FromQuery] string? redirectUri = null)
    {
        try
        {
            var authUrl = await _oauthService.GetAuthorizationUrlAsync(provider, redirectUri);
            return Ok(authUrl);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid OAuth provider requested: {Provider}", provider);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating OAuth authorization URL for provider: {Provider}", provider);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Handle OAuth callback and login user
    /// </summary>
    /// <param name="provider">OAuth provider</param>
    /// <param name="code">Authorization code from provider</param>
    /// <param name="state">State parameter for CSRF protection</param>
    /// <param name="redirectUri">Redirect URI used in authorization</param>
    /// <returns>Authentication result with JWT tokens</returns>
    [HttpPost("{provider}/callback")]
    public async Task<IActionResult> HandleCallback(
        string provider,
        [FromForm] string code,
        [FromForm] string? state = null,
        [FromForm] string? redirectUri = null)
    {
        try
        {
            var request = new OAuthLoginRequest
            {
                Provider = provider,
                Code = code,
                State = state,
                RedirectUri = redirectUri
            };

            var result = await _oauthService.LoginWithOAuthAsync(request);

            if (result.IsSuccess)
            {
                _logger.LogInformation("OAuth login successful for provider: {Provider}", provider);
                return Ok(result);
            }

            _logger.LogWarning("OAuth login failed for provider: {Provider}, Error: {Error}",
                provider, result.ErrorMessage);
            return BadRequest(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid OAuth callback request: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling OAuth callback for provider: {Provider}", provider);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Link OAuth account to existing user account
    /// </summary>
    /// <param name="provider">OAuth provider</param>
    /// <param name="request">OAuth login request</param>
    /// <returns>Success result</returns>
    [HttpPost("{provider}/link")]
    [Authorize]
    public async Task<IActionResult> LinkAccount(string provider, [FromBody] OAuthLoginRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            request.Provider = provider;
            var result = await _oauthService.LinkOAuthAccountAsync(userId, request);

            if (result.IsSuccess)
            {
                _logger.LogInformation("OAuth account linked successfully for user: {UserId}, Provider: {Provider}",
                    userId, provider);
                return Ok(new { message = "Account linked successfully" });
            }

            _logger.LogWarning("OAuth account linking failed for user: {UserId}, Provider: {Provider}, Error: {Error}",
                userId, provider, result.ErrorMessage);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking OAuth account for provider: {Provider}", provider);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Unlink OAuth account from user account
    /// </summary>
    /// <param name="provider">OAuth provider to unlink</param>
    /// <returns>Success result</returns>
    [HttpDelete("{provider}/unlink")]
    [Authorize]
    public async Task<IActionResult> UnlinkAccount(string provider)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var success = await _oauthService.UnlinkOAuthAccountAsync(userId, provider);

            if (success)
            {
                _logger.LogInformation("OAuth account unlinked successfully for user: {UserId}, Provider: {Provider}",
                    userId, provider);
                return Ok(new { message = "Account unlinked successfully" });
            }

            return BadRequest(new { message = "Failed to unlink account" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlinking OAuth account for provider: {Provider}", provider);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get linked OAuth providers for current user
    /// </summary>
    /// <returns>List of linked providers</returns>
    [HttpGet("linked")]
    [Authorize]
    public async Task<IActionResult> GetLinkedProviders()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var providers = await _oauthService.GetLinkedProvidersAsync(userId);
            return Ok(new { providers });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting linked OAuth providers");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}