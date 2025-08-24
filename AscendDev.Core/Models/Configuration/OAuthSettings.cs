namespace AscendDev.Core.Models.Configuration;

public class OAuthSettings
{
    public const string SectionName = "OAuth";

    public GitHubOAuthSettings GitHub { get; set; } = new();
    public GoogleOAuthSettings Google { get; set; } = new();
}

public class GitHubOAuthSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string Scope { get; set; } = "user:email";
    public string AuthorizationUrl { get; set; } = "https://github.com/login/oauth/authorize";
    public string TokenUrl { get; set; } = "https://github.com/login/oauth/access_token";
    public string UserInfoUrl { get; set; } = "https://api.github.com/user";
    public string UserEmailUrl { get; set; } = "https://api.github.com/user/emails";
}

public class GoogleOAuthSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string Scope { get; set; } = "openid profile email";
    public string AuthorizationUrl { get; set; } = "https://accounts.google.com/o/oauth2/v2/auth";
    public string TokenUrl { get; set; } = "https://oauth2.googleapis.com/token";
    public string UserInfoUrl { get; set; } = "https://www.googleapis.com/oauth2/v2/userinfo";
}