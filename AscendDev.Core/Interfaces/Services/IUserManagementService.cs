using AscendDev.Core.DTOs.Admin;
using AscendDev.Core.Models.Auth;

namespace AscendDev.Core.Interfaces.Services;

public interface IUserManagementService
{
    // User Management
    Task<PagedResult<UserManagementDto>> SearchUsersAsync(UserSearchRequest request);
    Task<UserManagementDto?> GetUserWithDetailsAsync(Guid userId);
    Task<UserManagementDto> CreateUserAsync(CreateUserRequest request);
    Task<UserManagementDto> UpdateUserAsync(Guid userId, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<bool> ActivateUserAsync(Guid userId);
    Task<bool> DeactivateUserAsync(Guid userId);
    Task<bool> LockUserAsync(Guid userId, DateTime? lockUntil = null);
    Task<bool> UnlockUserAsync(Guid userId);

    // Role Management
    Task<List<Role>> GetAllRolesAsync();
    Task<List<string>> GetUserRolesAsync(Guid userId);
    Task<bool> AssignRoleToUserAsync(Guid userId, string roleName);
    Task<bool> RemoveRoleFromUserAsync(Guid userId, string roleName);
    Task<bool> UpdateUserRolesAsync(Guid userId, List<string> roles);

    // Bulk Operations
    Task<BulkOperationResult> PerformBulkOperationAsync(BulkUserOperationRequest request, Guid performedBy);
    Task<BulkOperationResult?> GetBulkOperationResultAsync(Guid operationId);
    Task<List<BulkOperationResult>> GetBulkOperationsAsync(Guid? performedBy = null, int limit = 50);

    // Activity Monitoring
    Task<bool> LogUserActivityAsync(Guid userId, string activityType, string? description = null,
        Dictionary<string, object>? metadata = null, string? ipAddress = null, string? userAgent = null);
    Task<List<UserActivityLogDto>> GetUserActivityAsync(Guid userId, int limit = 100);
    Task<List<UserActivityLogDto>> GetRecentActivityAsync(int limit = 100);

    // User Statistics
    Task<UserStatisticsDto?> GetUserStatisticsAsync(Guid userId);
    Task<List<UserStatisticsDto>> GetTopUsersByPointsAsync(int limit = 10);
    Task<List<UserStatisticsDto>> GetTopUsersByStreakAsync(int limit = 10);
}