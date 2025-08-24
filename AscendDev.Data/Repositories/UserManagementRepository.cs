using AscendDev.Core.DTOs.Admin;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Admin;
using AscendDev.Core.Models.Auth;
using Dapper;
using System.Text;

namespace AscendDev.Data.Repositories;

public class UserManagementRepository : IUserManagementRepository
{
    private readonly ISqlExecutor _sqlExecutor;

    public UserManagementRepository(ISqlExecutor sqlExecutor)
    {
        _sqlExecutor = sqlExecutor;
    }

    public async Task<PagedResult<UserManagementDto>> SearchUsersAsync(UserSearchRequest request)
    {
        var whereConditions = new List<string>();
        var parameters = new DynamicParameters();

        // Build WHERE clause based on search criteria
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            whereConditions.Add("(u.username ILIKE @searchTerm OR u.email ILIKE @searchTerm OR u.first_name ILIKE @searchTerm OR u.last_name ILIKE @searchTerm)");
            parameters.Add("searchTerm", $"%{request.SearchTerm}%");
        }

        if (request.Roles?.Any() == true)
        {
            whereConditions.Add("EXISTS (SELECT 1 FROM user_roles ur JOIN roles r ON ur.role_id = r.id WHERE ur.user_id = u.id AND r.name = ANY(@roles))");
            parameters.Add("roles", request.Roles.ToArray());
        }

        if (request.IsActive.HasValue)
        {
            whereConditions.Add("u.is_active = @isActive");
            parameters.Add("isActive", request.IsActive.Value);
        }

        if (request.IsLocked.HasValue)
        {
            whereConditions.Add("u.is_locked = @isLocked");
            parameters.Add("isLocked", request.IsLocked.Value);
        }

        if (request.CreatedAfter.HasValue)
        {
            whereConditions.Add("u.created_at >= @createdAfter");
            parameters.Add("createdAfter", request.CreatedAfter.Value);
        }

        if (request.CreatedBefore.HasValue)
        {
            whereConditions.Add("u.created_at <= @createdBefore");
            parameters.Add("createdBefore", request.CreatedBefore.Value);
        }

        if (request.LastLoginAfter.HasValue)
        {
            whereConditions.Add("u.last_login >= @lastLoginAfter");
            parameters.Add("lastLoginAfter", request.LastLoginAfter.Value);
        }

        if (request.LastLoginBefore.HasValue)
        {
            whereConditions.Add("u.last_login <= @lastLoginBefore");
            parameters.Add("lastLoginBefore", request.LastLoginBefore.Value);
        }

        var whereClause = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : "";

        // Count query
        var countSql = $@"
            SELECT COUNT(DISTINCT u.id)
            FROM users u
            {whereClause}";

        var totalCount = await _sqlExecutor.QuerySingleAsync<int>(countSql, parameters);

        // Data query with pagination
        var offset = (request.Page - 1) * request.PageSize;
        parameters.Add("offset", offset);
        parameters.Add("pageSize", request.PageSize);

        var orderBy = request.SortBy switch
        {
            "Email" => "u.email",
            "Username" => "u.username",
            "FirstName" => "u.first_name",
            "LastName" => "u.last_name",
            "LastLogin" => "u.last_login",
            _ => "u.created_at"
        };

        var orderDirection = request.SortDirection.ToLower() == "asc" ? "ASC" : "DESC";

        var dataSql = $@"
            SELECT DISTINCT
                u.id,
                u.email,
                u.username,
                u.first_name,
                u.last_name,
                CONCAT(COALESCE(u.first_name, ''), ' ', COALESCE(u.last_name, '')) as full_name,
                u.is_email_verified,
                u.is_active,
                u.is_locked,
                u.locked_until,
                u.created_at,
                u.last_login,
                u.provider,
                COALESCE(
                    (SELECT ARRAY_AGG(r.name) 
                     FROM user_roles ur 
                     JOIN roles r ON ur.role_id = r.id 
                     WHERE ur.user_id = u.id), 
                    ARRAY[]::text[]
                ) as roles,
                us.total_points,
                us.lessons_completed,
                us.courses_completed,
                us.current_streak,
                us.longest_streak,
                us.last_activity_date
            FROM users u
            LEFT JOIN user_statistics us ON u.id = us.user_id
            {whereClause}
            ORDER BY {orderBy} {orderDirection}
            OFFSET @offset LIMIT @pageSize";

        var users = await _sqlExecutor.QueryAsync<UserManagementDto, UserStatisticsDto, UserManagementDto>(
            dataSql,
            (user, stats) =>
            {
                user.Statistics = stats;
                return user;
            },
            parameters,
            splitOn: "total_points"
        );

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new PagedResult<UserManagementDto>
        {
            Items = users.ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            HasNextPage = request.Page < totalPages,
            HasPreviousPage = request.Page > 1
        };
    }

    public async Task<UserManagementDto?> GetUserWithDetailsAsync(Guid userId)
    {
        var sql = @"
            SELECT 
                u.id,
                u.email,
                u.username,
                u.first_name,
                u.last_name,
                CONCAT(COALESCE(u.first_name, ''), ' ', COALESCE(u.last_name, '')) as full_name,
                u.is_email_verified,
                u.is_active,
                u.is_locked,
                u.locked_until,
                u.created_at,
                u.last_login,
                u.provider,
                COALESCE(
                    (SELECT ARRAY_AGG(r.name) 
                     FROM user_roles ur 
                     JOIN roles r ON ur.role_id = r.id 
                     WHERE ur.user_id = u.id), 
                    ARRAY[]::text[]
                ) as roles,
                us.total_points,
                us.lessons_completed,
                us.courses_completed,
                us.current_streak,
                us.longest_streak,
                us.last_activity_date
            FROM users u
            LEFT JOIN user_statistics us ON u.id = us.user_id
            WHERE u.id = @userId";

        var result = await _sqlExecutor.QueryAsync<UserManagementDto, UserStatisticsDto, UserManagementDto>(
            sql,
            (user, stats) =>
            {
                user.Statistics = stats;
                return user;
            },
            new { userId },
            splitOn: "total_points"
        );

        return result.FirstOrDefault();
    }

    public async Task<bool> CreateUserAsync(User user, List<string> roles)
    {
        var userSql = @"
            INSERT INTO users (id, email, password_hash, username, first_name, last_name, 
                             is_email_verified, created_at, is_active, language)
            VALUES (@Id, @Email, @PasswordHash, @Username, @FirstName, @LastName, 
                   @IsEmailVerified, @CreatedAt, @IsActive, @Language)";

        var success = await _sqlExecutor.ExecuteAsync(userSql, user) > 0;

        if (success && roles.Any())
        {
            await UpdateUserRolesAsync(user.Id, roles);
        }

        return success;
    }

    public async Task<bool> UpdateUserAsync(Guid userId, UpdateUserRequest request)
    {
        var setParts = new List<string>();
        var parameters = new DynamicParameters();
        parameters.Add("userId", userId);

        if (!string.IsNullOrEmpty(request.Email))
        {
            setParts.Add("email = @email");
            parameters.Add("email", request.Email);
        }

        if (!string.IsNullOrEmpty(request.Username))
        {
            setParts.Add("username = @username");
            parameters.Add("username", request.Username);
        }

        if (!string.IsNullOrEmpty(request.FirstName))
        {
            setParts.Add("first_name = @firstName");
            parameters.Add("firstName", request.FirstName);
        }

        if (!string.IsNullOrEmpty(request.LastName))
        {
            setParts.Add("last_name = @lastName");
            parameters.Add("lastName", request.LastName);
        }

        if (request.IsActive.HasValue)
        {
            setParts.Add("is_active = @isActive");
            parameters.Add("isActive", request.IsActive.Value);
        }

        if (request.IsLocked.HasValue)
        {
            setParts.Add("is_locked = @isLocked");
            parameters.Add("isLocked", request.IsLocked.Value);
            if (!request.IsLocked.Value)
            {
                setParts.Add("locked_until = NULL");
            }
        }

        if (!setParts.Any())
            return true;

        setParts.Add("updated_at = NOW()");

        var sql = $@"
            UPDATE users 
            SET {string.Join(", ", setParts)}
            WHERE id = @userId";

        var result = await _sqlExecutor.ExecuteAsync(sql, parameters) > 0;

        if (result && request.Roles != null)
        {
            await UpdateUserRolesAsync(userId, request.Roles);
        }

        return result;
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var sql = "DELETE FROM users WHERE id = @userId";
        return await _sqlExecutor.ExecuteAsync(sql, new { userId }) > 0;
    }

    public async Task<List<Role>> GetAllRolesAsync()
    {
        var sql = "SELECT id, name, description FROM roles ORDER BY name";
        var roles = await _sqlExecutor.QueryAsync<Role>(sql);
        return roles.ToList();
    }

    public async Task<List<string>> GetUserRolesAsync(Guid userId)
    {
        var sql = @"
            SELECT r.name
            FROM user_roles ur
            JOIN roles r ON ur.role_id = r.id
            WHERE ur.user_id = @userId
            ORDER BY r.name";

        var roles = await _sqlExecutor.QueryAsync<string>(sql, new { userId });
        return roles.ToList();
    }

    public async Task<bool> AssignRoleToUserAsync(Guid userId, string roleName)
    {
        var sql = @"
            INSERT INTO user_roles (user_id, role_id, assigned_at)
            SELECT @userId, r.id, NOW()
            FROM roles r
            WHERE r.name = @roleName
            AND NOT EXISTS (
                SELECT 1 FROM user_roles ur 
                WHERE ur.user_id = @userId AND ur.role_id = r.id
            )";

        return await _sqlExecutor.ExecuteAsync(sql, new { userId, roleName }) > 0;
    }

    public async Task<bool> RemoveRoleFromUserAsync(Guid userId, string roleName)
    {
        var sql = @"
            DELETE FROM user_roles
            WHERE user_id = @userId
            AND role_id = (SELECT id FROM roles WHERE name = @roleName)";

        return await _sqlExecutor.ExecuteAsync(sql, new { userId, roleName }) > 0;
    }

    public async Task<bool> UpdateUserRolesAsync(Guid userId, List<string> roles)
    {
        // Remove all existing roles
        var deleteSql = "DELETE FROM user_roles WHERE user_id = @userId";
        await _sqlExecutor.ExecuteAsync(deleteSql, new { userId });

        if (!roles.Any())
            return true;

        // Add new roles
        var insertSql = @"
            INSERT INTO user_roles (user_id, role_id, assigned_at)
            SELECT @userId, r.id, NOW()
            FROM roles r
            WHERE r.name = ANY(@roles)";

        return await _sqlExecutor.ExecuteAsync(insertSql, new { userId, roles = roles.ToArray() }) > 0;
    }

    public async Task<Guid> CreateBulkOperationAsync(BulkOperation operation)
    {
        var sql = @"
            INSERT INTO bulk_operations (id, operation_type, performed_by, target_count, 
                                       operation_data, status, started_at)
            VALUES (@Id, @OperationType, @PerformedBy, @TargetCount, 
                   @OperationData::jsonb, @Status, @StartedAt)
            RETURNING id";

        var id = await _sqlExecutor.QuerySingleAsync<Guid>(sql, operation);
        return id;
    }

    public async Task<bool> UpdateBulkOperationAsync(Guid operationId, string status, string? errorMessage = null)
    {
        var sql = @"
            UPDATE bulk_operations 
            SET status = @status, 
                completed_at = CASE WHEN @status IN ('completed', 'failed') THEN NOW() ELSE completed_at END,
                error_message = @errorMessage
            WHERE id = @operationId";

        return await _sqlExecutor.ExecuteAsync(sql, new { operationId, status, errorMessage }) > 0;
    }

    public async Task<BulkOperationResult?> GetBulkOperationResultAsync(Guid operationId)
    {
        var sql = @"
            SELECT id as operation_id, operation_type, target_count, 
                   target_count as success_count, 0 as failure_count,
                   status, started_at, completed_at
            FROM bulk_operations
            WHERE id = @operationId";

        return await _sqlExecutor.QuerySingleOrDefaultAsync<BulkOperationResult>(sql, new { operationId });
    }

    public async Task<List<BulkOperationResult>> GetBulkOperationsAsync(Guid? performedBy = null, int limit = 50)
    {
        var whereClause = performedBy.HasValue ? "WHERE performed_by = @performedBy" : "";

        var sql = $@"
            SELECT id as operation_id, operation_type, target_count,
                   target_count as success_count, 0 as failure_count,
                   status, started_at, completed_at
            FROM bulk_operations
            {whereClause}
            ORDER BY started_at DESC
            LIMIT @limit";

        var operations = await _sqlExecutor.QueryAsync<BulkOperationResult>(sql, new { performedBy, limit });
        return operations.ToList();
    }

    public async Task<bool> LogUserActivityAsync(UserActivityLog activity)
    {
        var sql = @"
            INSERT INTO user_activity_logs (id, user_id, activity_type, activity_description, 
                                          metadata, ip_address, user_agent, session_id, created_at)
            VALUES (@Id, @UserId, @ActivityType, @ActivityDescription, 
                   @Metadata::jsonb, @IpAddress, @UserAgent, @SessionId, @CreatedAt)";

        return await _sqlExecutor.ExecuteAsync(sql, activity) > 0;
    }

    public async Task<List<UserActivityLogDto>> GetUserActivityAsync(Guid userId, int limit = 100)
    {
        var sql = @"
            SELECT ual.id, ual.user_id, u.username, ual.activity_type, 
                   ual.activity_description, ual.metadata, ual.ip_address, ual.created_at
            FROM user_activity_logs ual
            JOIN users u ON ual.user_id = u.id
            WHERE ual.user_id = @userId
            ORDER BY ual.created_at DESC
            LIMIT @limit";

        var activities = await _sqlExecutor.QueryAsync<UserActivityLogDto>(sql, new { userId, limit });
        return activities.ToList();
    }

    public async Task<List<UserActivityLogDto>> GetRecentActivityAsync(int limit = 100)
    {
        var sql = @"
            SELECT ual.id, ual.user_id, u.username, ual.activity_type, 
                   ual.activity_description, ual.metadata, ual.ip_address, ual.created_at
            FROM user_activity_logs ual
            JOIN users u ON ual.user_id = u.id
            ORDER BY ual.created_at DESC
            LIMIT @limit";

        var activities = await _sqlExecutor.QueryAsync<UserActivityLogDto>(sql, new { limit });
        return activities.ToList();
    }

    public async Task<UserStatisticsDto?> GetUserStatisticsAsync(Guid userId)
    {
        var sql = @"
            SELECT total_points, lessons_completed, courses_completed, 
                   current_streak, longest_streak, last_activity_date
            FROM user_statistics
            WHERE user_id = @userId";

        return await _sqlExecutor.QuerySingleOrDefaultAsync<UserStatisticsDto>(sql, new { userId });
    }

    public async Task<List<UserStatisticsDto>> GetTopUsersByPointsAsync(int limit = 10)
    {
        var sql = @"
            SELECT us.total_points, us.lessons_completed, us.courses_completed, 
                   us.current_streak, us.longest_streak, us.last_activity_date
            FROM user_statistics us
            JOIN users u ON us.user_id = u.id
            WHERE u.is_active = true
            ORDER BY us.total_points DESC
            LIMIT @limit";

        var stats = await _sqlExecutor.QueryAsync<UserStatisticsDto>(sql, new { limit });
        return stats.ToList();
    }

    public async Task<List<UserStatisticsDto>> GetTopUsersByStreakAsync(int limit = 10)
    {
        var sql = @"
            SELECT us.total_points, us.lessons_completed, us.courses_completed, 
                   us.current_streak, us.longest_streak, us.last_activity_date
            FROM user_statistics us
            JOIN users u ON us.user_id = u.id
            WHERE u.is_active = true
            ORDER BY us.current_streak DESC, us.longest_streak DESC
            LIMIT @limit";

        var stats = await _sqlExecutor.QueryAsync<UserStatisticsDto>(sql, new { limit });
        return stats.ToList();
    }
}