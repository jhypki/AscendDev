namespace AscendDev.Core.Models.Auth;

public class UserSettings
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public bool PublicSubmissions { get; set; } = false;
    public bool ShowProfile { get; set; } = true;
    public bool EmailOnCodeReview { get; set; } = true;
    public bool EmailOnDiscussionReply { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;
}