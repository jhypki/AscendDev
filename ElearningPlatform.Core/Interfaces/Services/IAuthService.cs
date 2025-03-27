using ElearningPlatform.Core.DTOs.Auth;

namespace ElearningPlatform.Core.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegistrationRequest registerRequest);
    Task<AuthResult> LoginAsync(LoginRequest loginRequest);
    Task<string> RefreshTokenAsync(string token);
}