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
    IRoleRepository roleRepository,
    JwtSettings jwtSettings,
    IPasswordHasher passwordHasher,
    IJwtHelper jwtHelper,
    IHttpContextAccessor httpContextAccessor,
    IEmailService emailService,
    ILogger<AuthService> logger)
    : IAuthService
{
    public async Task<AuthResult> RegisterAsync(RegistrationRequest request)
    {
        var existingUser = await userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new ConflictException("User with this email already exists.");

        // Generate email verification token
        var verificationToken = GenerateEmailVerificationToken();
        var tokenExpiry = DateTime.UtcNow.AddHours(24); // Token expires in 24 hours

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = passwordHasher.Hash(request.Password),
            CreatedAt = DateTime.UtcNow,
            LastLogin = DateTime.UtcNow,
            IsEmailVerified = false,
            Username = request.Email.Split('@')[0],
            Provider = "Local", //TODO add this field to the database
            EmailVerificationToken = verificationToken,
            EmailVerificationTokenExpires = tokenExpiry
        };

        await userRepository.CreateAsync(user);

        // Assign default "user" role
        await AssignDefaultRoleAsync(user.Id);

        // Send email verification
        await SendEmailVerificationAsync(user);

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

        var (accessToken, newRefreshToken, roles) = await GenerateTokensAsync(user);

        await refreshTokenRepository.SaveAsync(newRefreshToken);

        await refreshTokenRepository.DeleteAsync(token);

        logger.LogInformation("Rotated refresh token for user {UserId}. Old token: {OldToken}, New token: {NewToken}",
            user.Id, token, newRefreshToken.Token);

        return AuthResult.Success(accessToken, newRefreshToken.Token, MapToUserDto(user, roles));
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

    public async Task LogoutAsync(string? refreshToken = null)
    {
        try
        {
            // If refresh token is provided, revoke it
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var token = await refreshTokenRepository.GetByTokenAsync(refreshToken);
                if (token != null)
                {
                    await refreshTokenRepository.RevokeAsync(refreshToken,
                        httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown");

                    logger.LogInformation("Refresh token revoked during logout for user {UserId}", token.UserId);
                }
            }

            // Additional logout logic can be added here (e.g., blacklist JWT tokens, clear sessions, etc.)
            logger.LogInformation("User logged out successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during logout process");
            // Don't throw - logout should succeed even if token revocation fails
        }
    }

    public async Task<AuthResult> GenerateAuthResultAsync(User user)
    {
        var (accessToken, refreshToken, roles) = await GenerateTokensAsync(user);
        await refreshTokenRepository.SaveAsync(refreshToken);

        return AuthResult.Success(accessToken, refreshToken.Token, MapToUserDto(user, roles));
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        try
        {
            return await userRepository.GetByIdAsync(userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user by ID {UserId}", userId);
            return null;
        }
    }

    public async Task<UserDto?> GetUserWithRolesByIdAsync(Guid userId)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
                return null;

            // Get user roles
            var userRoles = await userRoleRepository.GetRolesByUserIdAsync(user.Id);
            var roleNames = userRoles.Select(r => r.Name).ToList();

            return MapToUserDto(user, roleNames);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user with roles by ID {UserId}", userId);
            return null;
        }
    }

    private async Task<(string AccessToken, RefreshToken RefreshToken, List<string> Roles)> GenerateTokensAsync(User user)
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

        return (accessToken, refreshToken, roleNames);
    }

    private static UserDto MapToUserDto(User user, List<string>? roles = null)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            ProfilePictureUrl = user.ProfilePictureUrl,
            IsEmailVerified = user.IsEmailVerified,
            UserRoles = roles ?? new List<string>(),
            Bio = user.Bio,
            Provider = user.Provider,
            CreatedAt = user.CreatedAt,
            LastLogin = user.LastLogin
        };
    }

    private async Task AssignDefaultRoleAsync(Guid userId)
    {
        try
        {
            // Get the "User" role
            var userRole = await roleRepository.GetByNameAsync("User");
            if (userRole != null)
            {
                var userRoleAssignment = new UserRole
                {
                    UserId = userId,
                    RoleId = userRole.Id
                };

                await userRoleRepository.CreateAsync(userRoleAssignment);
                logger.LogInformation("Default 'User' role assigned to user {UserId}", userId);
            }
            else
            {
                logger.LogWarning("Default 'User' role not found in database for user {UserId}", userId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to assign default role to user {UserId}", userId);
        }
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        try
        {
            var user = await userRepository.GetByEmailVerificationTokenAsync(token);
            if (user == null)
            {
                logger.LogWarning("Email verification attempted with invalid token: {Token}", token);
                return false;
            }

            if (!user.IsEmailVerificationTokenValid)
            {
                logger.LogWarning("Email verification attempted with expired token for user: {UserId}", user.Id);
                return false;
            }

            // Mark email as verified and clear the verification token
            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpires = null;
            user.UpdatedAt = DateTime.UtcNow;

            await userRepository.UpdateAsync(user);

            // Send welcome email
            await emailService.SendWelcomeEmailAsync(user.Email, user.Username);

            logger.LogInformation("Email verified successfully for user: {UserId}", user.Id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verifying email with token: {Token}", token);
            return false;
        }
    }

    public async Task<bool> ResendEmailVerificationAsync(string email)
    {
        try
        {
            var user = await userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                logger.LogWarning("Email verification resend attempted for non-existent email: {Email}", email);
                return false;
            }

            if (user.IsEmailVerified)
            {
                logger.LogWarning("Email verification resend attempted for already verified user: {UserId}", user.Id);
                return false;
            }

            // Generate new verification token
            var verificationToken = GenerateEmailVerificationToken();
            var tokenExpiry = DateTime.UtcNow.AddHours(24);

            user.EmailVerificationToken = verificationToken;
            user.EmailVerificationTokenExpires = tokenExpiry;
            user.UpdatedAt = DateTime.UtcNow;

            await userRepository.UpdateAsync(user);

            // Send verification email
            await SendEmailVerificationAsync(user);

            logger.LogInformation("Email verification resent for user: {UserId}", user.Id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error resending email verification for email: {Email}", email);
            return false;
        }
    }

    private async Task SendEmailVerificationAsync(User user)
    {
        try
        {
            var baseUrl = GetBaseUrl();
            var verificationUrl = $"{baseUrl}/auth/verify-email?token={user.EmailVerificationToken}";

            await emailService.SendEmailVerificationAsync(user.Email, verificationUrl, user.Username);

            logger.LogInformation("Email verification sent to user: {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email verification to user: {UserId}", user.Id);
            // Don't throw - registration should succeed even if email fails
        }
    }

    private string GetBaseUrl()
    {
        // For email verification, we need to return the frontend URL, not the API URL
        // This should be configured in appsettings, but for now we'll use the development frontend URL
        var request = httpContextAccessor.HttpContext?.Request;
        if (request != null)
        {
            // Check if we're in development (localhost)
            if (request.Host.Host == "localhost")
            {
                // Return frontend URL for development
                return "http://localhost:3000";
            }
            // For production, return the frontend domain
            return "https://ascenddev.com";
        }

        // Fallback URL - frontend URL
        return "http://localhost:3000";
    }

    private static string GenerateEmailVerificationToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    private static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }
}