using AscendDev.Core.Models.Auth;

namespace AscendDev.Core.Models.Courses;

public class Submission
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string LessonId { get; set; } = null!;
    public string Code { get; set; } = null!;
    public bool Passed { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string? TestResults { get; set; } // JSON string containing test execution results
    public int ExecutionTimeMs { get; set; }
    public string? ErrorMessage { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
}