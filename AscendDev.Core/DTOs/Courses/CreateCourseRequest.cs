using System.ComponentModel.DataAnnotations;

namespace AscendDev.Core.DTOs.Courses;

public class CreateCourseRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = null!;

    [Required]
    [StringLength(100, MinimumLength = 3)]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug must contain only lowercase letters, numbers, and hyphens")]
    public string Slug { get; set; } = null!;

    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Description { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string Language { get; set; } = null!;

    public string? FeaturedImage { get; set; }

    public List<string> Tags { get; set; } = [];

    public string Status { get; set; } = "draft";
}