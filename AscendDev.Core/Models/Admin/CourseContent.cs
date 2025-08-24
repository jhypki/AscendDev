using AscendDev.Core.Models.Auth;
using AscendDev.Core.Models.Courses;

namespace AscendDev.Core.Models.Admin;

public class CourseContent
{
    public Guid Id { get; set; }
    public string CourseId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Language { get; set; } = null!;
    public DifficultyLevel DifficultyLevel { get; set; } = DifficultyLevel.Beginner;
    public int? EstimatedDurationMinutes { get; set; }
    public List<string> Prerequisites { get; set; } = new();
    public List<string> LearningObjectives { get; set; } = new();
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsPublished { get; set; } = false;
    public int Version { get; set; } = 1;

    // Navigation Properties
    public Course Course { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;

    // Computed Properties
    public bool IsRecent => CreatedAt > DateTime.UtcNow.AddDays(-7);
    public TimeSpan Age => DateTime.UtcNow - CreatedAt;
}

public enum DifficultyLevel
{
    Beginner,
    Intermediate,
    Advanced,
    Expert
}