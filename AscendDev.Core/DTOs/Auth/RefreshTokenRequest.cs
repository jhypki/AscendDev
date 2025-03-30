using System.ComponentModel.DataAnnotations;

namespace AscendDev.Core.DTOs.Auth;

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = null!;
}