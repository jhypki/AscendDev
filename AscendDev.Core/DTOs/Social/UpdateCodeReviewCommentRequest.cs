using System.ComponentModel.DataAnnotations;

namespace AscendDev.Core.DTOs.Social;

public class UpdateCodeReviewCommentRequest
{
    [StringLength(2000, MinimumLength = 5)]
    public string? Content { get; set; }

    public bool? IsResolved { get; set; }
}