using AscendDev.Core.Models.Courses;

namespace AscendDev.Core.DTOs.Courses;

public class LessonResponse
{
    public string Id { get; set; } = null!;
    public string CourseId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string Template { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Language { get; set; } = null!;
    public int Order { get; set; }
    public List<AdditionalResource> AdditionalResources { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public string MainFunction { get; set; } = null!;
    public List<TestCase> TestCases { get; set; } = [];
}