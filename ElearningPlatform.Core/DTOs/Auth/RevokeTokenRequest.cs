using System.ComponentModel.DataAnnotations;

namespace ElearningPlatform.Core.DTOs.Auth;

public class RevokeTokenRequest
{
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = null!;
}