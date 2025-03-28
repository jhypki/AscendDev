namespace ElearningPlatform.Core.DTOs.Auth;

public class AuthResult
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public UserDto User { get; set; } = null!;
}