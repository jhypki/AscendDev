using System.ComponentModel.DataAnnotations;

namespace AscendDev.Core.DTOs.Social;

public class CreateCodeReviewCommentRequest
{
    [Required]
    [StringLength(2000, MinimumLength = 5)]
    public string Content { get; set; } = null!;

    public int? LineNumber { get; set; }
}