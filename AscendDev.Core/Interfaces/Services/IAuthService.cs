using AscendDev.Core.DTOs.Auth;
using AscendDev.Core.Models.Auth;

namespace AscendDev.Core.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegistrationRequest registerRequest);

    Task<AuthResult> LoginAsync(LoginRequest loginRequest);

    Task<AuthResult> RefreshTokenAsync(string token);

    Task RevokeRefreshTokenAsync(string token);

    Task<AuthResult> GenerateAuthResultAsync(User user);
}