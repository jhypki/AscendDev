namespace ElearningPlatform.Core.Entities.Auth;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string? PasswordHash { get; set; }
    public string Username { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Bio { get; set; }
    public string? ExternalId { get; set; } = null;
    public string? Provider { get; set; } = null;
}