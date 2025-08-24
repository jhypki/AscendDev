namespace AscendDev.Core.DTOs.Auth;

public class OAuthLoginRequest
{
    public string Provider { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? State { get; set; }
    public string? RedirectUri { get; set; }
}

public class OAuthUserInfo
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Name { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Username { get; set; }
    public string? AvatarUrl { get; set; }
    public string Provider { get; set; } = null!;
    public Dictionary<string, object> RawData { get; set; } = new();
}

public class OAuthTokenResponse
{
    public string AccessToken { get; set; } = null!;
    public string? RefreshToken { get; set; }
    public string TokenType { get; set; } = "Bearer";
    public int? ExpiresIn { get; set; }
    public string? Scope { get; set; }
}

public class OAuthAuthorizationUrl
{
    public string Url { get; set; } = null!;
    public string State { get; set; } = null!;
}