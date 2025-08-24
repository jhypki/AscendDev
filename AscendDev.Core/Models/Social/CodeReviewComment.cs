using AscendDev.Core.Models.Auth;

namespace AscendDev.Core.Models.Social;

public class CodeReviewComment
{
    public Guid Id { get; set; }
    public Guid CodeReviewId { get; set; }
    public Guid UserId { get; set; }
    public int? LineNumber { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsResolved { get; set; } = false;

    // Navigation Properties
    public CodeReview CodeReview { get; set; } = null!;
    public User User { get; set; } = null!;

    // Computed Properties
    public bool IsEdited => UpdatedAt.HasValue && UpdatedAt.Value > CreatedAt.AddMinutes(1);
    public bool IsLineComment => LineNumber.HasValue;
    public bool IsGeneralComment => !LineNumber.HasValue;
}