namespace AscendDev.Core.DTOs.Courses;

public class SubmissionResponse
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string LessonId { get; set; } = null!;
    public string Code { get; set; } = null!;
    public bool Passed { get; set; }
    public DateTime SubmittedAt { get; set; }
    public int ExecutionTimeMs { get; set; }
    public string? ErrorMessage { get; set; }

    // User information
    public string Username { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfilePictureUrl { get; set; }

    // Lesson information
    public string LessonTitle { get; set; } = null!;
    public string LessonSlug { get; set; } = null!;
}

public class PublicSubmissionResponse
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string LessonId { get; set; } = null!;
    public string Code { get; set; } = null!;
    public DateTime SubmittedAt { get; set; }
    public int ExecutionTimeMs { get; set; }

    // User information (limited for privacy)
    public string Username { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? ProfilePictureUrl { get; set; }

    // Lesson information
    public string LessonTitle { get; set; } = null!;
    public string LessonSlug { get; set; } = null!;
}

public class UserSettingsResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public bool PublicSubmissions { get; set; }
    public bool ShowProfile { get; set; }
    public bool EmailOnCodeReview { get; set; }
    public bool EmailOnDiscussionReply { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpdateUserSettingsRequest
{
    public bool PublicSubmissions { get; set; }
    public bool ShowProfile { get; set; }
    public bool EmailOnCodeReview { get; set; }
    public bool EmailOnDiscussionReply { get; set; }
}