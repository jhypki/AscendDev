using AscendDev.Core.DTOs.Auth;

namespace AscendDev.Core.DTOs.Social;

public class DiscussionResponse
{
    public Guid Id { get; set; }
    public string LessonId { get; set; } = null!;
    public Guid UserId { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsPinned { get; set; }
    public bool IsLocked { get; set; }
    public int ViewCount { get; set; }
    public int ReplyCount { get; set; }
    public DateTime LastActivity { get; set; }
    public UserDto User { get; set; } = null!;
    public List<DiscussionReplyResponse>? Replies { get; set; }
}