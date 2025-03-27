using ElearningPlatform.Core.Entities.Auth;

namespace ElearningPlatform.Core.Interfaces.Data;

public interface IUserRoleRepository
{
    Task<bool> CreateAsync(UserRole userRole);
    Task<bool> DeleteAsync(UserRole userRole);
    Task<IEnumerable<Role>> GetRolesByUserIdAsync(Guid userId);
}