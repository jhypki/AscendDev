namespace AscendDev.Core.Models.Courses;

public class UserLessonCode
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string LessonId { get; set; } = null!;
    public string Code { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}