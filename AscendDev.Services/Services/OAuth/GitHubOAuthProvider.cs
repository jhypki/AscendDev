using AscendDev.Core.DTOs.Auth;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;
using System.Security.Cryptography;

namespace AscendDev.Services.Services.OAuth;

public class GitHubOAuthProvider : IOAuthProvider
{
    public string ProviderName => "github";

    private readonly GitHubOAuthSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<GitHubOAuthProvider> _logger;

    public GitHubOAuthProvider(
        IOptions<OAuthSettings> oauthSettings,
        HttpClient httpClient,
        ILogger<GitHubOAuthProvider> logger)
    {
        _settings = oauthSettings.Value.GitHub;
        _httpClient = httpClient;
        _logger = logger;
    }

    public Task<OAuthAuthorizationUrl> GetAuthorizationUrlAsync(string? redirectUri = null)
    {
        var state = GenerateSecureState();
        var actualRedirectUri = redirectUri ?? _settings.RedirectUri;

        var queryParams = new Dictionary<string, string>
        {
            ["client_id"] = _settings.ClientId,
            ["redirect_uri"] = actualRedirectUri,
            ["scope"] = _settings.Scope,
            ["state"] = state,
            ["response_type"] = "code"
        };

        var queryString = string.Join("&", queryParams.Select(kvp =>
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        var authUrl = $"{_settings.AuthorizationUrl}?{queryString}";

        return Task.FromResult(new OAuthAuthorizationUrl
        {
            Url = authUrl,
            State = state
        });
    }

    public async Task<OAuthTokenResponse> ExchangeCodeForTokenAsync(string code, string? redirectUri = null)
    {
        var actualRedirectUri = redirectUri ?? _settings.RedirectUri;

        var tokenRequest = new Dictionary<string, string>
        {
            ["client_id"] = _settings.ClientId,
            ["client_secret"] = _settings.ClientSecret,
            ["code"] = code,
            ["redirect_uri"] = actualRedirectUri
        };

        var content = new FormUrlEncodedContent(tokenRequest);

        var request = new HttpRequestMessage(HttpMethod.Post, _settings.TokenUrl)
        {
            Content = content
        };
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("User-Agent", "AscendDev-OAuth-Client");

        try
        {
            _logger.LogDebug("Exchanging GitHub OAuth code for token. RedirectUri: {RedirectUri}", actualRedirectUri);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("GitHub token response status: {StatusCode}, Content: {Content}",
                response.StatusCode, responseContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("GitHub OAuth token request failed with status {StatusCode}: {Content}",
                    response.StatusCode, responseContent);
                throw new InvalidOperationException($"GitHub OAuth request failed with status {response.StatusCode}");
            }

            var tokenData = JsonSerializer.Deserialize<JsonElement>(responseContent);

            if (tokenData.TryGetProperty("error", out var error))
            {
                var errorDescription = tokenData.TryGetProperty("error_description", out var desc)
                    ? desc.GetString() : "Unknown error";
                _logger.LogError("GitHub OAuth error response: {Error} - {Description}",
                    error.GetString(), errorDescription);
                throw new InvalidOperationException($"GitHub OAuth error: {error.GetString()} - {errorDescription}");
            }

            if (!tokenData.TryGetProperty("access_token", out var accessTokenProperty))
            {
                _logger.LogError("GitHub OAuth response missing access_token: {Content}", responseContent);
                throw new InvalidOperationException("GitHub OAuth response missing access_token");
            }

            var accessToken = accessTokenProperty.GetString();
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("GitHub OAuth returned empty access_token");
                throw new InvalidOperationException("GitHub OAuth returned empty access_token");
            }

            return new OAuthTokenResponse
            {
                AccessToken = accessToken,
                TokenType = tokenData.TryGetProperty("token_type", out var tokenType)
                    ? tokenType.GetString()! : "Bearer",
                Scope = tokenData.TryGetProperty("scope", out var scope)
                    ? scope.GetString() : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to exchange GitHub OAuth code for token. Code: {Code}, RedirectUri: {RedirectUri}",
                code?.Substring(0, Math.Min(code.Length, 10)) + "...", actualRedirectUri);
            throw new InvalidOperationException("Failed to obtain access token from GitHub", ex);
        }
    }

    public async Task<OAuthUserInfo> GetUserInfoAsync(string accessToken)
    {
        try
        {
            // Get user profile
            var userRequest = new HttpRequestMessage(HttpMethod.Get, _settings.UserInfoUrl);
            userRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
            userRequest.Headers.Add("User-Agent", "AscendDev-OAuth-Client");

            var userResponse = await _httpClient.SendAsync(userRequest);
            userResponse.EnsureSuccessStatusCode();

            var userContent = await userResponse.Content.ReadAsStringAsync();
            var userData = JsonSerializer.Deserialize<JsonElement>(userContent);

            // Get user emails
            var emailRequest = new HttpRequestMessage(HttpMethod.Get, _settings.UserEmailUrl);
            emailRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
            emailRequest.Headers.Add("User-Agent", "AscendDev-OAuth-Client");

            var emailResponse = await _httpClient.SendAsync(emailRequest);
            emailResponse.EnsureSuccessStatusCode();

            var emailContent = await emailResponse.Content.ReadAsStringAsync();
            var emailData = JsonSerializer.Deserialize<JsonElement>(emailContent);

            // Find primary email
            var primaryEmail = string.Empty;
            if (emailData.ValueKind == JsonValueKind.Array)
            {
                foreach (var email in emailData.EnumerateArray())
                {
                    if (email.TryGetProperty("primary", out var isPrimary) && isPrimary.GetBoolean())
                    {
                        primaryEmail = email.GetProperty("email").GetString()!;
                        break;
                    }
                }
            }

            // Fallback to first email if no primary found
            if (string.IsNullOrEmpty(primaryEmail) && emailData.ValueKind == JsonValueKind.Array)
            {
                var firstEmail = emailData.EnumerateArray().FirstOrDefault();
                if (firstEmail.ValueKind != JsonValueKind.Undefined)
                {
                    primaryEmail = firstEmail.GetProperty("email").GetString()!;
                }
            }

            var name = userData.TryGetProperty("name", out var nameProperty)
                ? nameProperty.GetString() : null;
            var nameParts = name?.Split(' ', 2) ?? new string[0];

            return new OAuthUserInfo
            {
                Id = userData.GetProperty("id").GetInt32().ToString(),
                Email = primaryEmail,
                Name = name,
                FirstName = nameParts.Length > 0 ? nameParts[0] : null,
                LastName = nameParts.Length > 1 ? nameParts[1] : null,
                Username = userData.TryGetProperty("login", out var login)
                    ? login.GetString() : null,
                AvatarUrl = userData.TryGetProperty("avatar_url", out var avatar)
                    ? avatar.GetString() : null,
                Provider = ProviderName,
                RawData = JsonSerializer.Deserialize<Dictionary<string, object>>(userContent)
                    ?? new Dictionary<string, object>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get GitHub user info");
            throw new InvalidOperationException("Failed to retrieve user information from GitHub", ex);
        }
    }

    private static string GenerateSecureState()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
}