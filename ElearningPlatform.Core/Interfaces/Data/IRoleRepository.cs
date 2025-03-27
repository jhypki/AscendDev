using ElearningPlatform.Core.Entities.Auth;

namespace ElearningPlatform.Core.Interfaces.Data;

public interface IRoleRepository
{
    Task<Role> GetByIdAsync(Guid id);
    Task<Role> GetByNameAsync(string name);
    Task<IEnumerable<Role>> GetAllAsync();
}