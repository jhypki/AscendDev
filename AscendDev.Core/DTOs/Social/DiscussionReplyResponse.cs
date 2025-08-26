using AscendDev.Core.DTOs.Auth;

namespace AscendDev.Core.DTOs.Social;

public class DiscussionReplyResponse
{
    public Guid Id { get; set; }
    public Guid DiscussionId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? ParentReplyId { get; set; }
    public bool IsSolution { get; set; }
    public bool IsEdited { get; set; }
    public int Depth { get; set; }
    public UserDto User { get; set; } = null!;
    public List<DiscussionReplyResponse>? ChildReplies { get; set; }
}