using AscendDev.Core.Entities.Auth;

namespace AscendDev.Core.Interfaces.Data;

public interface IRoleRepository
{
    Task<Role> GetByIdAsync(Guid id);
    Task<Role> GetByNameAsync(string name);
    Task<IEnumerable<Role>> GetAllAsync();
}