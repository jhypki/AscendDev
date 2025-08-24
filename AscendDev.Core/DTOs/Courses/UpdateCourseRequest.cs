using System.ComponentModel.DataAnnotations;

namespace AscendDev.Core.DTOs.Courses;

public class UpdateCourseRequest
{
    [StringLength(200, MinimumLength = 3)]
    public string? Title { get; set; }

    [StringLength(100, MinimumLength = 3)]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug must contain only lowercase letters, numbers, and hyphens")]
    public string? Slug { get; set; }

    [StringLength(2000, MinimumLength = 10)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? Language { get; set; }

    public string? FeaturedImage { get; set; }

    public List<string>? Tags { get; set; }

    public string? Status { get; set; }
}