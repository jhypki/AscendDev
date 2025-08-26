using AscendDev.Core.Models.Auth;
using AscendDev.Core.Models.Courses;

namespace AscendDev.Core.Models.Social;

public class CodeReview
{
    public Guid Id { get; set; }
    public string LessonId { get; set; } = null!;
    public Guid ReviewerId { get; set; }
    public Guid RevieweeId { get; set; }
    public int SubmissionId { get; set; }
    public CodeReviewStatus Status { get; set; } = CodeReviewStatus.Pending;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation Properties
    public User Reviewer { get; set; } = null!;
    public User Reviewee { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
    public Submission Submission { get; set; } = null!;
    public List<CodeReviewComment> Comments { get; set; } = new();

    // Computed Properties
    public bool IsCompleted => Status == CodeReviewStatus.Approved || Status == CodeReviewStatus.Completed;
    public int CommentCount => Comments.Count;
    public TimeSpan? ReviewDuration => CompletedAt.HasValue ? CompletedAt.Value - CreatedAt : null;
}

public enum CodeReviewStatus
{
    Pending,
    InReview,
    ChangesRequested,
    Approved,
    Completed
}