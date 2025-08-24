using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Auth;

namespace AscendDev.Data.Repositories;

public class UserRoleRepository : IUserRoleRepository
{
    private readonly ISqlExecutor _sqlExecutor;

    public UserRoleRepository(ISqlExecutor sqlExecutor)
    {
        _sqlExecutor = sqlExecutor;
    }

    public async Task<bool> CreateAsync(UserRole userRole)
    {
        const string sql = @"
            INSERT INTO user_roles (user_id, role_id)
            VALUES (@UserId, @RoleId)";

        var rowsAffected = await _sqlExecutor.ExecuteAsync(sql, userRole);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(UserRole userRole)
    {
        const string sql = @"
            DELETE FROM user_roles 
            WHERE user_id = @UserId AND role_id = @RoleId";

        var rowsAffected = await _sqlExecutor.ExecuteAsync(sql, userRole);
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<Role>> GetRolesByUserIdAsync(Guid userId)
    {
        const string sql = @"
            SELECT r.id, r.name, r.description
            FROM roles r
            INNER JOIN user_roles ur ON r.id = ur.role_id
            WHERE ur.user_id = @UserId";

        var roles = await _sqlExecutor.QueryAsync<Role>(sql, new { UserId = userId });
        return roles;
    }
}