namespace AscendDev.Core.Models.Courses;

public class Course
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

    // Versioning support
    public int Version { get; set; } = 1;
    public string? ParentCourseId { get; set; }
    public bool IsPublished { get; set; } = false;
    public DateTime? PublishedAt { get; set; }
    public Guid? PublishedBy { get; set; }

    // Audit fields
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Analytics fields
    public int ViewCount { get; set; } = 0;
    public int EnrollmentCount { get; set; } = 0;
    public double Rating { get; set; } = 0.0;
    public int RatingCount { get; set; } = 0;

    // Content validation
    public bool IsValidated { get; set; } = false;
    public DateTime? ValidatedAt { get; set; }
    public Guid? ValidatedBy { get; set; }
    public List<string> ValidationErrors { get; set; } = [];
}