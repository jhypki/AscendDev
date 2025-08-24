namespace AscendDev.Core.Models.Auth;

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

    // OAuth and External Authentication
    public string? ExternalId { get; set; }
    public string? Provider { get; set; } // github, google, etc.

    // Enhanced Authentication Features
    public bool IsActive { get; set; } = true;
    public bool IsLocked { get; set; } = false;
    public DateTime? LockedUntil { get; set; }
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LastFailedLogin { get; set; }
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpires { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpires { get; set; }

    // User Preferences
    public string? TimeZone { get; set; }
    public string? Language { get; set; } = "en";
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;

    // Navigation Properties
    public List<UserRole> UserRoles { get; set; } = new();
    public List<RefreshToken> RefreshTokens { get; set; } = new();

    // Computed Properties
    public string FullName => $"{FirstName} {LastName}".Trim();
    public bool IsEmailVerificationTokenValid =>
        !string.IsNullOrEmpty(EmailVerificationToken) &&
        EmailVerificationTokenExpires.HasValue &&
        EmailVerificationTokenExpires.Value > DateTime.UtcNow;
    public bool IsPasswordResetTokenValid =>
        !string.IsNullOrEmpty(PasswordResetToken) &&
        PasswordResetTokenExpires.HasValue &&
        PasswordResetTokenExpires.Value > DateTime.UtcNow;
    public bool IsAccountLocked =>
        IsLocked && LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;
}