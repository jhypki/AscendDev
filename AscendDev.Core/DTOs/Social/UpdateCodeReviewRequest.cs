using AscendDev.Core.Models.Social;

namespace AscendDev.Core.DTOs.Social;

public class UpdateCodeReviewRequest
{
    public CodeReviewStatus? Status { get; set; }

    public int? SubmissionId { get; set; }
}