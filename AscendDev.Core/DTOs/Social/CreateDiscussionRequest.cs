using System.ComponentModel.DataAnnotations;

namespace AscendDev.Core.DTOs.Social;

public class CreateDiscussionRequest
{
    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Title { get; set; } = null!;

    [Required]
    [StringLength(5000, MinimumLength = 10)]
    public string Content { get; set; } = null!;

    public string? LessonId { get; set; }

    public string? CourseId { get; set; }
}