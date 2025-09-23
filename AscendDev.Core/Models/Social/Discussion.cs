using AscendDev.Core.Models.Auth;
using AscendDev.Core.Models.Courses;

namespace AscendDev.Core.Models.Social;

public class Discussion
{
    public Guid Id { get; set; }
    public string? LessonId { get; set; }
    public string? CourseId { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsPinned { get; set; } = false;
    public bool IsLocked { get; set; } = false;
    public int ViewCount { get; set; } = 0;

    // Navigation Properties
    public User User { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
    public List<DiscussionReply> Replies { get; set; } = new();
    public List<DiscussionLike> Likes { get; set; } = new();

    // Computed Properties
    public int ReplyCount => Replies.Count;
    public int LikeCount => Likes.Count;
    public DateTime LastActivity => Replies.Any() ?
        Replies.Max(r => r.UpdatedAt ?? r.CreatedAt) :
        UpdatedAt ?? CreatedAt;
}