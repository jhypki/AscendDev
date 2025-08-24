using AscendDev.Core.DTOs.Auth;

namespace AscendDev.Core.Interfaces.Services;

public interface IOAuthService
{
    Task<OAuthAuthorizationUrl> GetAuthorizationUrlAsync(string provider, string? redirectUri = null);
    Task<OAuthUserInfo> GetUserInfoAsync(string provider, string code, string? redirectUri = null);
    Task<AuthResult> LoginWithOAuthAsync(OAuthLoginRequest request);
    Task<AuthResult> LinkOAuthAccountAsync(Guid userId, OAuthLoginRequest request);
    Task<bool> UnlinkOAuthAccountAsync(Guid userId, string provider);
    Task<List<string>> GetLinkedProvidersAsync(Guid userId);
}

public interface IOAuthProvider
{
    string ProviderName { get; }
    Task<OAuthAuthorizationUrl> GetAuthorizationUrlAsync(string? redirectUri = null);
    Task<OAuthTokenResponse> ExchangeCodeForTokenAsync(string code, string? redirectUri = null);
    Task<OAuthUserInfo> GetUserInfoAsync(string accessToken);
}