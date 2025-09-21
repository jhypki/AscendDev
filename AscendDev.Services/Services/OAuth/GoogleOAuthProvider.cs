using AscendDev.Core.DTOs.Auth;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Security.Cryptography;

namespace AscendDev.Services.Services.OAuth;

public class GoogleOAuthProvider : IOAuthProvider
{
    public string ProviderName => "google";

    private readonly GoogleOAuthSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleOAuthProvider> _logger;

    public GoogleOAuthProvider(
        IOptions<OAuthSettings> oauthSettings,
        HttpClient httpClient,
        ILogger<GoogleOAuthProvider> logger)
    {
        _settings = oauthSettings.Value.Google;
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
            ["response_type"] = "code",
            ["access_type"] = "offline",
            ["prompt"] = "consent"
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
            ["grant_type"] = "authorization_code",
            ["redirect_uri"] = actualRedirectUri
        };

        var content = new FormUrlEncodedContent(tokenRequest);

        var request = new HttpRequestMessage(HttpMethod.Post, _settings.TokenUrl)
        {
            Content = content
        };
        request.Headers.Add("Accept", "application/json");

        try
        {
            _logger.LogDebug("Exchanging Google OAuth code for token. RedirectUri: {RedirectUri}", actualRedirectUri);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("Google token response status: {StatusCode}, Content: {Content}",
                response.StatusCode, responseContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Google OAuth token request failed with status {StatusCode}: {Content}",
                    response.StatusCode, responseContent);
                throw new InvalidOperationException($"Google OAuth request failed with status {response.StatusCode}");
            }

            var tokenData = JsonSerializer.Deserialize<JsonElement>(responseContent);

            if (tokenData.TryGetProperty("error", out var error))
            {
                var errorDescription = tokenData.TryGetProperty("error_description", out var desc)
                    ? desc.GetString() : "Unknown error";
                _logger.LogError("Google OAuth error response: {Error} - {Description}",
                    error.GetString(), errorDescription);
                throw new InvalidOperationException($"Google OAuth error: {error.GetString()} - {errorDescription}");
            }

            if (!tokenData.TryGetProperty("access_token", out var accessTokenProperty))
            {
                _logger.LogError("Google OAuth response missing access_token: {Content}", responseContent);
                throw new InvalidOperationException("Google OAuth response missing access_token");
            }

            var accessToken = accessTokenProperty.GetString();
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("Google OAuth returned empty access_token");
                throw new InvalidOperationException("Google OAuth returned empty access_token");
            }

            return new OAuthTokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = tokenData.TryGetProperty("refresh_token", out var refreshToken)
                    ? refreshToken.GetString() : null,
                TokenType = tokenData.TryGetProperty("token_type", out var tokenType)
                    ? tokenType.GetString()! : "Bearer",
                ExpiresIn = tokenData.TryGetProperty("expires_in", out var expiresIn)
                    ? expiresIn.GetInt32() : null,
                Scope = tokenData.TryGetProperty("scope", out var scope)
                    ? scope.GetString() : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to exchange Google OAuth code for token. Code: {Code}, RedirectUri: {RedirectUri}",
                code?.Substring(0, Math.Min(code.Length, 10)) + "...", actualRedirectUri);
            throw new InvalidOperationException("Failed to obtain access token from Google", ex);
        }
    }

    public async Task<OAuthUserInfo> GetUserInfoAsync(string accessToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _settings.UserInfoUrl);
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var userData = JsonSerializer.Deserialize<JsonElement>(content);

            if (userData.TryGetProperty("error", out var error))
            {
                throw new InvalidOperationException($"Google API error: {error.GetString()}");
            }

            var name = userData.TryGetProperty("name", out var nameProperty)
                ? nameProperty.GetString() : null;

            return new OAuthUserInfo
            {
                Id = userData.GetProperty("id").GetString()!,
                Email = userData.TryGetProperty("email", out var email)
                    ? email.GetString()! : string.Empty,
                Name = name,
                FirstName = userData.TryGetProperty("given_name", out var firstName)
                    ? firstName.GetString() : null,
                LastName = userData.TryGetProperty("family_name", out var lastName)
                    ? lastName.GetString() : null,
                Username = userData.TryGetProperty("email", out var emailForUsername)
                    ? emailForUsername.GetString()?.Split('@')[0] : null,
                AvatarUrl = userData.TryGetProperty("picture", out var picture)
                    ? picture.GetString() : null,
                Provider = ProviderName,
                RawData = JsonSerializer.Deserialize<Dictionary<string, object>>(content)
                    ?? new Dictionary<string, object>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Google user info");
            throw new InvalidOperationException("Failed to retrieve user information from Google", ex);
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