using AscendDev.Core.Models.Auth;

namespace AscendDev.Core.Models.Social;

public class CodeReviewComment
{
    public Guid Id { get; set; }
    public Guid CodeReviewId { get; set; }
    public Guid UserId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public int? LineNumber { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsResolved { get; set; } = false;

    // Navigation Properties
    public CodeReview CodeReview { get; set; } = null!;
    public User User { get; set; } = null!;
    public CodeReviewComment? ParentComment { get; set; }
    public ICollection<CodeReviewComment> Replies { get; set; } = new List<CodeReviewComment>();

    // Computed Properties
    public bool IsEdited => UpdatedAt.HasValue && UpdatedAt.Value > CreatedAt.AddMinutes(1);
    public bool IsLineComment => LineNumber.HasValue;
    public bool IsGeneralComment => !LineNumber.HasValue;
    public bool IsReply => ParentCommentId.HasValue;
    public int ReplyCount => Replies?.Count ?? 0;
}