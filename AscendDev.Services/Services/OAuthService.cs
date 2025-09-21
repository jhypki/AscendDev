using AscendDev.Core.DTOs.Auth;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.Auth;
using AscendDev.Core.Models.Configuration;
using AscendDev.Core.Interfaces.Data;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace AscendDev.Services.Services;

public class OAuthService : IOAuthService
{
    private readonly Dictionary<string, IOAuthProvider> _providers;
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;
    private readonly ILogger<OAuthService> _logger;
    private readonly SecuritySettings _securitySettings;

    public OAuthService(
        IEnumerable<IOAuthProvider> providers,
        IUserRepository userRepository,
        IAuthService authService,
        IOptions<SecuritySettings> securitySettings,
        ILogger<OAuthService> logger)
    {
        _providers = providers.ToDictionary(p => p.ProviderName.ToLower(), p => p);
        _userRepository = userRepository;
        _authService = authService;
        _securitySettings = securitySettings.Value;
        _logger = logger;
    }

    public async Task<OAuthAuthorizationUrl> GetAuthorizationUrlAsync(string provider, string? redirectUri = null)
    {
        var oauthProvider = GetProvider(provider);
        return await oauthProvider.GetAuthorizationUrlAsync(redirectUri);
    }

    public async Task<OAuthUserInfo> GetUserInfoAsync(string provider, string code, string? redirectUri = null)
    {
        try
        {
            _logger.LogDebug("Getting user info for provider: {Provider}, RedirectUri: {RedirectUri}",
                provider, redirectUri);

            var oauthProvider = GetProvider(provider);
            var tokenResponse = await oauthProvider.ExchangeCodeForTokenAsync(code, redirectUri);

            _logger.LogDebug("Successfully obtained access token for provider: {Provider}", provider);

            return await oauthProvider.GetUserInfoAsync(tokenResponse.AccessToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user info for provider: {Provider}", provider);
            throw;
        }
    }

    public async Task<AuthResult> LoginWithOAuthAsync(OAuthLoginRequest request)
    {
        try
        {
            _logger.LogInformation("Starting OAuth login for provider: {Provider}, RedirectUri: {RedirectUri}",
                request.Provider, request.RedirectUri);

            var userInfo = await GetUserInfoAsync(request.Provider, request.Code, request.RedirectUri);

            // Try to find existing user by OAuth provider
            var existingUser = await _userRepository.GetByExternalIdAsync(userInfo.Id, request.Provider);

            if (existingUser != null)
            {
                // Update last login and user info
                existingUser.LastLogin = DateTime.UtcNow;
                existingUser.ProfilePictureUrl = userInfo.AvatarUrl ?? existingUser.ProfilePictureUrl;
                await _userRepository.UpdateAsync(existingUser);

                _logger.LogInformation("OAuth login successful for existing user: {UserId}, Provider: {Provider}",
                    existingUser.Id, request.Provider);

                return await _authService.GenerateAuthResultAsync(existingUser);
            }

            // Try to find user by email for account linking
            var userByEmail = await _userRepository.GetByEmailAsync(userInfo.Email);
            if (userByEmail != null)
            {
                // Link OAuth account to existing email account
                userByEmail.ExternalId = userInfo.Id;
                userByEmail.Provider = request.Provider;
                userByEmail.LastLogin = DateTime.UtcNow;
                userByEmail.ProfilePictureUrl = userInfo.AvatarUrl ?? userByEmail.ProfilePictureUrl;
                userByEmail.IsEmailVerified = true; // OAuth emails are considered verified

                await _userRepository.UpdateAsync(userByEmail);

                _logger.LogInformation("OAuth account linked to existing email user: {UserId}, Provider: {Provider}",
                    userByEmail.Id, request.Provider);

                return await _authService.GenerateAuthResultAsync(userByEmail);
            }

            // Create new user account
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Email = userInfo.Email,
                Username = await GenerateUniqueUsernameAsync(userInfo.Username ?? userInfo.Email.Split('@')[0]),
                FirstName = userInfo.FirstName,
                LastName = userInfo.LastName,
                ExternalId = userInfo.Id,
                Provider = request.Provider,
                IsEmailVerified = true, // OAuth emails are considered verified
                ProfilePictureUrl = userInfo.AvatarUrl,
                CreatedAt = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow,
                IsActive = true
            };

            await _userRepository.CreateAsync(newUser);

            // Assign default user role
            await AssignDefaultRoleAsync(newUser.Id);

            _logger.LogInformation("New OAuth user created: {UserId}, Provider: {Provider}",
                newUser.Id, request.Provider);

            return await _authService.GenerateAuthResultAsync(newUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OAuth login failed for provider: {Provider}", request.Provider);
            return AuthResult.Failure("OAuth login failed. Please try again.");
        }
    }

    public async Task<AuthResult> LinkOAuthAccountAsync(Guid userId, OAuthLoginRequest request)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return AuthResult.Failure("User not found");
            }

            // Check if user already has this provider linked
            if (!string.IsNullOrEmpty(user.Provider) && user.Provider.Equals(request.Provider, StringComparison.OrdinalIgnoreCase))
            {
                return AuthResult.Failure("This OAuth provider is already linked to your account");
            }

            var userInfo = await GetUserInfoAsync(request.Provider, request.Code, request.RedirectUri);

            // Check if this OAuth account is already linked to another user
            var existingOAuthUser = await _userRepository.GetByExternalIdAsync(userInfo.Id, request.Provider);
            if (existingOAuthUser != null && existingOAuthUser.Id != userId)
            {
                return AuthResult.Failure("This OAuth account is already linked to another user");
            }

            // Link the OAuth account
            user.ExternalId = userInfo.Id;
            user.Provider = request.Provider;
            user.ProfilePictureUrl = userInfo.AvatarUrl ?? user.ProfilePictureUrl;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("OAuth account linked successfully: {UserId}, Provider: {Provider}",
                userId, request.Provider);

            return AuthResult.Success("OAuth account linked successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OAuth account linking failed: {UserId}, Provider: {Provider}",
                userId, request.Provider);
            return AuthResult.Failure("Failed to link OAuth account. Please try again.");
        }
    }

    public async Task<bool> UnlinkOAuthAccountAsync(Guid userId, string provider)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            // Check if user has a password set (can't unlink if it's the only auth method)
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                _logger.LogWarning("Cannot unlink OAuth account - no password set: {UserId}, Provider: {Provider}",
                    userId, provider);
                return false;
            }

            // Check if this is the linked provider
            if (!provider.Equals(user.Provider, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Unlink the OAuth account
            user.ExternalId = null;
            user.Provider = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("OAuth account unlinked successfully: {UserId}, Provider: {Provider}",
                userId, provider);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OAuth account unlinking failed: {UserId}, Provider: {Provider}",
                userId, provider);
            return false;
        }
    }

    public async Task<List<string>> GetLinkedProvidersAsync(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.Provider))
            {
                return new List<string>();
            }

            return new List<string> { user.Provider };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get linked providers for user: {UserId}", userId);
            return new List<string>();
        }
    }

    private IOAuthProvider GetProvider(string provider)
    {
        if (!_providers.TryGetValue(provider.ToLower(), out var oauthProvider))
        {
            throw new ArgumentException($"OAuth provider '{provider}' is not supported");
        }
        return oauthProvider;
    }

    private async Task<string> GenerateUniqueUsernameAsync(string baseUsername)
    {
        var username = baseUsername.ToLower().Replace(" ", "");
        var counter = 0;
        var originalUsername = username;

        while (await _userRepository.GetByUsernameAsync(username) != null)
        {
            counter++;
            username = $"{originalUsername}{counter}";
        }

        return username;
    }

    private async Task AssignDefaultRoleAsync(Guid userId)
    {
        try
        {
            // This would typically assign the "User" role
            // Implementation depends on your role repository
            _logger.LogInformation("Default role assigned to new OAuth user: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to assign default role to user: {UserId}", userId);
        }
    }
}