using AscendDev.Core.Models.Social;
using System.ComponentModel.DataAnnotations;

namespace AscendDev.Core.DTOs.Social;

public class UpdateCodeReviewRequest
{
    public CodeReviewStatus? Status { get; set; }

    [StringLength(10000, MinimumLength = 10)]
    public string? CodeSolution { get; set; }
}