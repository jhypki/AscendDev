using System.ComponentModel.DataAnnotations;

namespace AscendDev.Core.DTOs.Social;

public class CreateCodeReviewRequest
{
    [Required]
    public string LessonId { get; set; } = null!;

    [Required]
    public Guid RevieweeId { get; set; }

    [Required]
    public int SubmissionId { get; set; }
}