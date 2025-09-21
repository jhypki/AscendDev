using AscendDev.Core.DTOs.Auth;
using AscendDev.Core.Models.Auth;

namespace AscendDev.Core.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegistrationRequest registerRequest);

    Task<AuthResult> LoginAsync(LoginRequest loginRequest);

    Task<AuthResult> RefreshTokenAsync(string token);

    Task RevokeRefreshTokenAsync(string token);

    Task LogoutAsync(string? refreshToken = null);

    Task<AuthResult> GenerateAuthResultAsync(User user);

    Task<User?> GetUserByIdAsync(Guid userId);

    Task<UserDto?> GetUserWithRolesByIdAsync(Guid userId);
}