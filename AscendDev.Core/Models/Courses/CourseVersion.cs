namespace AscendDev.Core.Models.Courses;

public class CourseVersion
{
    public string Id { get; set; } = null!;

    public string CourseId { get; set; } = null!;

    public int VersionNumber { get; set; }

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Language { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string? FeaturedImage { get; set; }

    public List<string> Tags { get; set; } = [];

    public List<LessonSummary> LessonSummaries { get; set; } = [];

    public string Status { get; set; } = "draft";

    public string CreatedBy { get; set; } = null!;

    public string? ChangeLog { get; set; }

    public bool IsActive { get; set; } = false;
}