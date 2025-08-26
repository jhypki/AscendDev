using AscendDev.Core.DTOs.Auth;

namespace AscendDev.Core.DTOs.Social;

public class CodeReviewCommentResponse
{
    public Guid Id { get; set; }
    public Guid CodeReviewId { get; set; }
    public Guid UserId { get; set; }
    public int? LineNumber { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsResolved { get; set; }
    public bool IsEdited { get; set; }
    public bool IsLineComment { get; set; }
    public bool IsGeneralComment { get; set; }
    public UserDto User { get; set; } = null!;
}