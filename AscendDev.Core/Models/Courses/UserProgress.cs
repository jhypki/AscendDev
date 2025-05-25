using AscendDev.Core.Models.Auth;

namespace AscendDev.Core.Models.Courses;

public class UserProgress
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string LessonId { get; set; } = null!;
    public DateTime CompletedAt { get; set; }
    public string? CodeSolution { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public Lesson? Lesson { get; set; }
}