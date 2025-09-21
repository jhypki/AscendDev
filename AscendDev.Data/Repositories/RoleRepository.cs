using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Auth;

namespace AscendDev.Data.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly ISqlExecutor _sqlExecutor;

    public RoleRepository(ISqlExecutor sqlExecutor)
    {
        _sqlExecutor = sqlExecutor;
    }

    public async Task<Role> GetByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT id, name, description
            FROM roles
            WHERE id = @Id";

        var role = await _sqlExecutor.QuerySingleOrDefaultAsync<Role>(sql, new { Id = id });
        return role;
    }

    public async Task<Role> GetByNameAsync(string name)
    {
        const string sql = @"
            SELECT id, name, description
            FROM roles
            WHERE name = @Name";

        var role = await _sqlExecutor.QuerySingleOrDefaultAsync<Role>(sql, new { Name = name });
        return role;
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        const string sql = @"
            SELECT id, name, description
            FROM roles
            ORDER BY name";

        var roles = await _sqlExecutor.QueryAsync<Role>(sql);
        return roles;
    }
}