using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AscendDev.Core.DTOs.UserProfile;

public class UpdateUserProfileRequest
{
    [JsonPropertyName("firstName")]
    [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
    public string? LastName { get; set; }

    [JsonPropertyName("bio")]
    [StringLength(1000, ErrorMessage = "Bio cannot exceed 1000 characters")]
    public string? Bio { get; set; }

    [JsonPropertyName("profilePictureUrl")]
    [Url(ErrorMessage = "Profile picture URL must be a valid URL")]
    [StringLength(500, ErrorMessage = "Profile picture URL cannot exceed 500 characters")]
    public string? ProfilePictureUrl { get; set; }
}

public class UserActivityFeedRequest
{
    [JsonPropertyName("userId")]
    public Guid? UserId { get; set; } // If null, gets current user's activity

    [JsonPropertyName("limit")]
    [Range(1, 100, ErrorMessage = "Limit must be between 1 and 100")]
    public int Limit { get; set; } = 20;

    [JsonPropertyName("offset")]
    [Range(0, int.MaxValue, ErrorMessage = "Offset must be non-negative")]
    public int Offset { get; set; } = 0;

    [JsonPropertyName("activityTypes")]
    public List<string>? ActivityTypes { get; set; } // Filter by activity types

    [JsonPropertyName("fromDate")]
    public DateTime? FromDate { get; set; }

    [JsonPropertyName("toDate")]
    public DateTime? ToDate { get; set; }
}

public class UserActivityFeedResponse
{
    [JsonPropertyName("activities")]
    public List<UserActivityResponse> Activities { get; set; } = new();

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("hasMore")]
    public bool HasMore { get; set; }

    [JsonPropertyName("nextOffset")]
    public int? NextOffset { get; set; }
}

public class UserProfileSearchRequest
{
    [JsonPropertyName("query")]
    [StringLength(100, ErrorMessage = "Search query cannot exceed 100 characters")]
    public string? Query { get; set; }

    [JsonPropertyName("limit")]
    [Range(1, 50, ErrorMessage = "Limit must be between 1 and 50")]
    public int Limit { get; set; } = 10;

    [JsonPropertyName("offset")]
    [Range(0, int.MaxValue, ErrorMessage = "Offset must be non-negative")]
    public int Offset { get; set; } = 0;

    [JsonPropertyName("sortBy")]
    public string SortBy { get; set; } = "username"; // username, points, joinDate, activity

    [JsonPropertyName("sortOrder")]
    public string SortOrder { get; set; } = "asc"; // asc, desc
}

public class UserProfileSearchResponse
{
    [JsonPropertyName("profiles")]
    public List<UserProfileSummary> Profiles { get; set; } = new();

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("hasMore")]
    public bool HasMore { get; set; }

    [JsonPropertyName("nextOffset")]
    public int? NextOffset { get; set; }
}

public class UserProfileSummary
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
    public string FullName => $"{FirstName} {LastName}".Trim();

    [JsonPropertyName("profilePictureUrl")]
    public string? ProfilePictureUrl { get; set; }

    [JsonPropertyName("bio")]
    public string? Bio { get; set; }

    [JsonPropertyName("totalPoints")]
    public int TotalPoints { get; set; }

    [JsonPropertyName("lessonsCompleted")]
    public int LessonsCompleted { get; set; }

    [JsonPropertyName("coursesCompleted")]
    public int CoursesCompleted { get; set; }

    [JsonPropertyName("currentStreak")]
    public int CurrentStreak { get; set; }

    [JsonPropertyName("joinedDate")]
    public DateTime JoinedDate { get; set; }

    [JsonPropertyName("lastActivityDate")]
    public DateTime? LastActivityDate { get; set; }
}