namespace AscendDev.Core.DTOs.Admin
{
    public class UserManagementResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CoursesEnrolled { get; set; }
        public int LessonsCompleted { get; set; }
    }

    public class PaginatedUserManagementResponse
    {
        public List<UserManagementResponse> Users { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }

    public class UserManagementQueryRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
    }
}