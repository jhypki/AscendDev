namespace AscendDev.Core.Models.Auth;

public class JwtSettings
{
    public string Key { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int AccessTokenExpiryMinutes { get; set; }
    public int RefreshTokenExpiryDays { get; set; }
}