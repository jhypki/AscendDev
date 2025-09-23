using AscendDev.Core.Models.Auth;

namespace AscendDev.Core.Models.Social;

public class DiscussionLike
{
    public Guid Id { get; set; }
    public Guid DiscussionId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public Discussion Discussion { get; set; } = null!;
    public User User { get; set; } = null!;
}