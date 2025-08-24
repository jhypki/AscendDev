using System.Security.Cryptography;
using AscendDev.Core.DTOs.Auth;
using AscendDev.Core.Exceptions;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Interfaces.Utils;
using AscendDev.Core.Models.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AscendDev.Services.Services;

public class AuthService(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IUserRoleRepository userRoleRepository,
    JwtSettings jwtSettings,
    IPasswordHasher passwordHasher,
    IJwtHelper jwtHelper,
    IHttpContextAccessor httpContextAccessor,
    ILogger<AuthService> logger)
    : IAuthService
{
    public async Task<AuthResult> RegisterAsync(RegistrationRequest request)
    {
        var existingUser = await userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new ConflictException("User with this email already exists.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = passwordHasher.Hash(request.Password),
            CreatedAt = DateTime.UtcNow,
            LastLogin = DateTime.UtcNow,
            IsEmailVerified = false,
            Username = request.Email.Split('@')[0],
            Provider = "Local" //TODO add this field to the database
        };

        await userRepository.CreateAsync(user);
        var authResult = await GenerateAuthResultAsync(user);

        return authResult;
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);
        if (user == null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new BadRequestException("Invalid email or password.");

        user.LastLogin = DateTime.UtcNow;
        await userRepository.UpdateAsync(user);

        var authResult = await GenerateAuthResultAsync(user);
        return authResult;
    }

    public async Task<AuthResult> RefreshTokenAsync(string token)
    {
        var refreshToken = await refreshTokenRepository.GetByTokenAsync(token);
        if (refreshToken is not { IsActive: true })
            throw new UnauthorizedException("Invalid or expired refresh token.");

        var user = await userRepository.GetByIdAsync(refreshToken.UserId);
        if (user == null)
            throw new NotFoundException("User not found.");

        var (accessToken, newRefreshToken) = await GenerateTokensAsync(user);

        await refreshTokenRepository.SaveAsync(newRefreshToken);

        await refreshTokenRepository.DeleteAsync(token);

        logger.LogInformation("Rotated refresh token for user {UserId}. Old token: {OldToken}, New token: {NewToken}",
            user.Id, token, newRefreshToken.Token);

        return AuthResult.Success(accessToken, newRefreshToken.Token, MapToUserDto(user));
    }

    public async Task RevokeRefreshTokenAsync(string token)
    {
        var refreshToken = await refreshTokenRepository.GetByTokenAsync(token);
        if (refreshToken is null)
            throw new NotFoundException("Refresh token");

        try
        {
            await refreshTokenRepository.RevokeAsync(token,
                httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error revoking refresh token");
            throw;
        }
    }

    public async Task<AuthResult> GenerateAuthResultAsync(User user)
    {
        var (accessToken, refreshToken) = await GenerateTokensAsync(user);
        await refreshTokenRepository.SaveAsync(refreshToken);

        return AuthResult.Success(accessToken, refreshToken.Token, MapToUserDto(user));
    }

    private async Task<(string AccessToken, RefreshToken RefreshToken)> GenerateTokensAsync(User user)
    {
        // Get user roles
        var userRoles = await userRoleRepository.GetRolesByUserIdAsync(user.Id);
        var roleNames = userRoles.Select(r => r.Name).ToList();

        var accessToken = jwtHelper.GenerateToken(user.Id, user.Email, roleNames);
        var refreshTokenString = GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenString,
            Expires = DateTime.UtcNow.AddDays(jwtSettings.RefreshTokenExpiryDays),
            Created = DateTime.UtcNow,
            CreatedByIp = httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            Revoked = null,
            RevokedByIp = null
        };

        return (accessToken, refreshToken);
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username
        };
    }

    private static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }
}