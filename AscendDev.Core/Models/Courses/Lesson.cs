using AscendDev.Core.Models.TestsExecution.CodeTemplates;

namespace AscendDev.Core.Models.Courses;

public class Lesson
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string CourseId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string Language { get; set; } = null!;

    /// <summary>
    /// Code template with editable/non-editable regions
    /// </summary>
    public CodeTemplate CodeTemplate { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int Order { get; set; }

    public TestConfig TestConfig { get; set; } = null!;

    public List<AdditionalResource> AdditionalResources { get; set; } = [];

    public List<string> Tags { get; set; } = [];

    public string Status { get; set; } = "draft";
}