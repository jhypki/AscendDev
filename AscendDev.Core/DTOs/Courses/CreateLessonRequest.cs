using System.ComponentModel.DataAnnotations;
using AscendDev.Core.Models.Courses;

namespace AscendDev.Core.DTOs.Courses;

public class CreateLessonRequest
{
    [Required]
    public string CourseId { get; set; } = null!;

    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = null!;

    [Required]
    [StringLength(100, MinimumLength = 3)]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug must contain only lowercase letters, numbers, and hyphens")]
    public string Slug { get; set; } = null!;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public string Content { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string Language { get; set; } = null!;

    [Required]
    public string Template { get; set; } = null!;

    [Required]
    public string TestTemplate { get; set; } = "123";

    public int Order { get; set; } = 1;

    public int OrderIndex { get; set; } = 1;

    [StringLength(50)]
    public string? Difficulty { get; set; }

    public int? EstimatedDuration { get; set; }

    public List<string> Prerequisites { get; set; } = [];

    public List<string> LearningObjectives { get; set; } = [];

    public List<object> CodeExamples { get; set; } = [];

    public List<object> Exercises { get; set; } = [];

    public TestConfig TestConfig { get; set; } = new();

    public List<AdditionalResource> AdditionalResources { get; set; } = [];

    public List<string> Tags { get; set; } = [];

    public string Status { get; set; } = "draft";
}