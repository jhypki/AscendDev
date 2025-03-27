using System.ComponentModel.DataAnnotations;

namespace ElearningPlatform.Core.DTOs.Auth;

public class RegistrationRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email is not valid")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(50, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 50 characters")]
    public string Password { get; set; } = null!;
}