using System.ComponentModel.DataAnnotations;
using AscendDev.Core.Models.Courses;

namespace AscendDev.Core.DTOs.Courses;

public class UpdateLessonRequest
{
    [StringLength(200, MinimumLength = 3)]
    public string? Title { get; set; }

    [StringLength(100, MinimumLength = 3)]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug must contain only lowercase letters, numbers, and hyphens")]
    public string? Slug { get; set; }

    [StringLength(10000, MinimumLength = 10)]
    public string? Content { get; set; }

    [StringLength(50)]
    public string? Language { get; set; }

    public string? Template { get; set; }

    [Range(1, int.MaxValue)]
    public int? Order { get; set; }

    public TestConfig? TestConfig { get; set; }

    public List<AdditionalResource>? AdditionalResources { get; set; }

    public List<string>? Tags { get; set; }

    public string? Status { get; set; }
}