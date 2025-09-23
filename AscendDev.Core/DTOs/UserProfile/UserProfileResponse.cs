using System.Text.Json.Serialization;

namespace AscendDev.Core.DTOs.UserProfile;

public class UserProfileResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; } = null!;

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("fullName")]
    public string FullName => $"{FirstName ?? ""} {LastName ?? ""}".Trim();

    [JsonPropertyName("email")]
    public string? Email { get; set; } // Only shown for own profile

    [JsonPropertyName("bio")]
    public string? Bio { get; set; }

    [JsonPropertyName("profilePictureUrl")]
    public string? ProfilePictureUrl { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("lastLogin")]
    public DateTime? LastLogin { get; set; }

    [JsonPropertyName("isEmailVerified")]
    public bool IsEmailVerified { get; set; }

    [JsonPropertyName("provider")]
    public string? Provider { get; set; }

    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; } = new();

    // Statistics
    [JsonPropertyName("statistics")]
    public UserProfileStatistics Statistics { get; set; } = new();

    // Settings (only for own profile)
    [JsonPropertyName("settings")]
    public UserProfileSettings? Settings { get; set; }

    // Recent Activity
    [JsonPropertyName("recentActivity")]
    public List<UserActivityResponse> RecentActivity { get; set; } = new();

    // Achievements
    [JsonPropertyName("achievements")]
    public List<UserAchievementResponse> Achievements { get; set; } = new();
}

public class UserProfileStatistics
{
    [JsonPropertyName("totalPoints")]
    public int TotalPoints { get; set; }

    [JsonPropertyName("lessonsCompleted")]
    public int LessonsCompleted { get; set; }

    [JsonPropertyName("coursesCompleted")]
    public int CoursesCompleted { get; set; }

    [JsonPropertyName("currentStreak")]
    public int CurrentStreak { get; set; }

    [JsonPropertyName("longestStreak")]
    public int LongestStreak { get; set; }

    [JsonPropertyName("totalCodeReviews")]
    public int TotalCodeReviews { get; set; }

    [JsonPropertyName("totalDiscussions")]
    public int TotalDiscussions { get; set; }

    [JsonPropertyName("lastActivityDate")]
    public DateTime? LastActivityDate { get; set; }

    [JsonPropertyName("joinedDate")]
    public DateTime JoinedDate { get; set; }
}

public class UserProfileSettings
{
    [JsonPropertyName("publicSubmissions")]
    public bool PublicSubmissions { get; set; }

    [JsonPropertyName("showProfile")]
    public bool ShowProfile { get; set; }

    [JsonPropertyName("emailOnCodeReview")]
    public bool EmailOnCodeReview { get; set; }

    [JsonPropertyName("emailOnDiscussionReply")]
    public bool EmailOnDiscussionReply { get; set; }
}

public class UserActivityResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = null!;

    [JsonPropertyName("description")]
    public string Description { get; set; } = null!;

    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("relatedEntityId")]
    public string? RelatedEntityId { get; set; }

    [JsonPropertyName("relatedEntityType")]
    public string? RelatedEntityType { get; set; }
}

public class UserAchievementResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("description")]
    public string Description { get; set; } = null!;

    [JsonPropertyName("iconUrl")]
    public string? IconUrl { get; set; }

    [JsonPropertyName("points")]
    public int Points { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; } = null!;

    [JsonPropertyName("earnedAt")]
    public DateTime EarnedAt { get; set; }

    [JsonPropertyName("progressData")]
    public Dictionary<string, object>? ProgressData { get; set; }
}