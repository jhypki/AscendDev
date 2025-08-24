using AscendDev.Core.Models.Auth;

namespace AscendDev.Core.DTOs.Admin;

public class UserManagementDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string FullName { get; set; } = null!;
    public bool IsEmailVerified { get; set; }
    public bool IsActive { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LockedUntil { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
    public string? Provider { get; set; }
    public string[] Roles { get; set; } = Array.Empty<string>();
    public UserStatisticsDto? Statistics { get; set; }
}

public class UserStatisticsDto
{
    public int TotalPoints { get; set; }
    public int LessonsCompleted { get; set; }
    public int CoursesCompleted { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateTime? LastActivityDate { get; set; }
}

public class CreateUserRequest
{
    public string Email { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Password { get; set; }
    public List<string> Roles { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public bool SendWelcomeEmail { get; set; } = true;
}

public class UpdateUserRequest
{
    public string? Email { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsLocked { get; set; }
    public List<string>? Roles { get; set; }
}

public class BulkUserOperationRequest
{
    public List<Guid> UserIds { get; set; } = new();
    public string Operation { get; set; } = null!; // activate, deactivate, lock, unlock, delete, assign_role, remove_role
    public Dictionary<string, object>? Parameters { get; set; }
}

public class BulkOperationResult
{
    public Guid OperationId { get; set; }
    public string OperationType { get; set; } = null!;
    public int TargetCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public string Status { get; set; } = null!;
    public List<string> Errors { get; set; } = new();
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class UserActivityLogDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = null!;
    public string ActivityType { get; set; } = null!;
    public string? ActivityDescription { get; set; }
    public string? Metadata { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserSearchRequest
{
    public string? SearchTerm { get; set; }
    public List<string>? Roles { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsLocked { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public DateTime? LastLoginAfter { get; set; }
    public DateTime? LastLoginBefore { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "CreatedAt";
    public string SortDirection { get; set; } = "desc";
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}