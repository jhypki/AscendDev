using AscendDev.Core.Models.Auth;
using AscendDev.Core.Models.Courses;

namespace AscendDev.Core.Models.Admin;

public class LessonContent
{
    public Guid Id { get; set; }
    public string CourseId { get; set; } = null!;
    public string? LessonId { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string? CodeTemplate { get; set; }
    public string? SolutionCode { get; set; }
    public List<string> Hints { get; set; } = new();
    public DifficultyLevel DifficultyLevel { get; set; } = DifficultyLevel.Beginner;
    public int? EstimatedDurationMinutes { get; set; }
    public int OrderIndex { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsPublished { get; set; } = false;
    public int Version { get; set; } = 1;

    // Navigation Properties
    public Course Course { get; set; } = null!;
    public Lesson? Lesson { get; set; }
    public User CreatedByUser { get; set; } = null!;

    // Computed Properties
    public bool IsRecent => CreatedAt > DateTime.UtcNow.AddDays(-7);
    public bool HasHints => Hints.Any();
    public bool HasSolution => !string.IsNullOrEmpty(SolutionCode);
    public bool HasTemplate => !string.IsNullOrEmpty(CodeTemplate);
}