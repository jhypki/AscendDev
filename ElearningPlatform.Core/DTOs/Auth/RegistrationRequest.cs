namespace ElearningPlatform.Core.DTOs.Auth;

public class RegistrationRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}