using AscendDev.Core.DTOs.Auth;
using AscendDev.Core.Models.Social;

namespace AscendDev.Core.DTOs.Social;

public class CodeReviewResponse
{
    public Guid Id { get; set; }
    public string LessonId { get; set; } = null!;
    public Guid ReviewerId { get; set; }
    public Guid RevieweeId { get; set; }
    public string CodeSolution { get; set; } = null!;
    public CodeReviewStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsCompleted { get; set; }
    public int CommentCount { get; set; }
    public TimeSpan? ReviewDuration { get; set; }
    public UserDto Reviewer { get; set; } = null!;
    public UserDto Reviewee { get; set; } = null!;
    public List<CodeReviewCommentResponse>? Comments { get; set; }
}