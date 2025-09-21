namespace AscendDev.Core.DTOs.Auth;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public bool IsEmailVerified { get; set; }
    public List<string> UserRoles { get; set; } = new();
    public string? Bio { get; set; }
    public string? Provider { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }

    // Computed Properties
    public string FullName => $"{FirstName} {LastName}".Trim();
}