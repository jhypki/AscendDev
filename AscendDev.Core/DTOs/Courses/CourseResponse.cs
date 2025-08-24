using AscendDev.Core.Models.Courses;

namespace AscendDev.Core.DTOs.Courses;

public class CourseResponse
{
    public string Id { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string Language { get; set; } = null!;
    public DateTime? UpdatedAt { get; set; }
    public string? FeaturedImage { get; set; }
    public List<string> Tags { get; set; } = [];
    public List<LessonSummary> LessonSummaries { get; set; } = [];
    public string Status { get; set; } = "draft";
    public int Version { get; set; } = 1;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}