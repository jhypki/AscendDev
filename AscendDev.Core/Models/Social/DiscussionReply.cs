using AscendDev.Core.Models.Auth;

namespace AscendDev.Core.Models.Social;

public class DiscussionReply
{
    public Guid Id { get; set; }
    public Guid DiscussionId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? ParentReplyId { get; set; }
    public bool IsSolution { get; set; } = false;

    // Navigation Properties
    public Discussion Discussion { get; set; } = null!;
    public User User { get; set; } = null!;
    public DiscussionReply? ParentReply { get; set; }
    public List<DiscussionReply> ChildReplies { get; set; } = new();

    // Computed Properties
    public bool IsEdited => UpdatedAt.HasValue && UpdatedAt.Value > CreatedAt.AddMinutes(1);
    public int Depth => ParentReply?.Depth + 1 ?? 0;
}