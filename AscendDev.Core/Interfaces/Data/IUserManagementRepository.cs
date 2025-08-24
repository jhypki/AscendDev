using AscendDev.Core.DTOs.Admin;
using AscendDev.Core.Models.Admin;
using AscendDev.Core.Models.Auth;

namespace AscendDev.Core.Interfaces.Data;

public interface IUserManagementRepository
{
    // User Management
    Task<PagedResult<UserManagementDto>> SearchUsersAsync(UserSearchRequest request);
    Task<UserManagementDto?> GetUserWithDetailsAsync(Guid userId);
    Task<bool> CreateUserAsync(User user, List<string> roles);
    Task<bool> UpdateUserAsync(Guid userId, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(Guid userId);

    // Role Management
    Task<List<Role>> GetAllRolesAsync();
    Task<List<string>> GetUserRolesAsync(Guid userId);
    Task<bool> AssignRoleToUserAsync(Guid userId, string roleName);
    Task<bool> RemoveRoleFromUserAsync(Guid userId, string roleName);
    Task<bool> UpdateUserRolesAsync(Guid userId, List<string> roles);

    // Bulk Operations
    Task<Guid> CreateBulkOperationAsync(BulkOperation operation);
    Task<bool> UpdateBulkOperationAsync(Guid operationId, string status, string? errorMessage = null);
    Task<BulkOperationResult?> GetBulkOperationResultAsync(Guid operationId);
    Task<List<BulkOperationResult>> GetBulkOperationsAsync(Guid? performedBy = null, int limit = 50);

    // Activity Logging
    Task<bool> LogUserActivityAsync(UserActivityLog activity);
    Task<List<UserActivityLogDto>> GetUserActivityAsync(Guid userId, int limit = 100);
    Task<List<UserActivityLogDto>> GetRecentActivityAsync(int limit = 100);

    // User Statistics
    Task<UserStatisticsDto?> GetUserStatisticsAsync(Guid userId);
    Task<List<UserStatisticsDto>> GetTopUsersByPointsAsync(int limit = 10);
    Task<List<UserStatisticsDto>> GetTopUsersByStreakAsync(int limit = 10);
}