using System.ComponentModel.DataAnnotations;

namespace AscendDev.Core.DTOs.Auth;

public class ResendVerificationRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email is not valid")]
    public string Email { get; set; } = null!;
}