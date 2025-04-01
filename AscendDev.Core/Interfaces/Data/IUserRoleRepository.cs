using AscendDev.Core.Models.Auth;

namespace AscendDev.Core.Interfaces.Data;

public interface IUserRoleRepository
{
    Task<bool> CreateAsync(UserRole userRole);
    Task<bool> DeleteAsync(UserRole userRole);
    Task<IEnumerable<Role>> GetRolesByUserIdAsync(Guid userId);
}