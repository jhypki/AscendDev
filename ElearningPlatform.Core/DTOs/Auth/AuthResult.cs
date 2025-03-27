namespace ElearningPlatform.Core.DTOs.Auth;

public class AuthResult
{
    public bool Success { get; set; }
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public UserDto User { get; set; } = null!;
}