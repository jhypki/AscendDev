using AscendDev.Core.DTOs.Auth;

namespace AscendDev.Core.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegistrationRequest registerRequest);

    Task<AuthResult> LoginAsync(LoginRequest loginRequest);

    // Task<AuthResult> OAuthLoginAsync(string provider, string accessToken);
    Task<AuthResult> RefreshTokenAsync(string token);

    Task RevokeRefreshTokenAsync(string token);
}