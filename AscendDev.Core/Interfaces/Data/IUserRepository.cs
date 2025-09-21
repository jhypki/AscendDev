using AscendDev.Core.Models.Auth;
using System.Linq.Expressions;

namespace AscendDev.Core.Interfaces.Data;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByExternalIdAsync(string externalId, string provider);
    Task<bool> CreateAsync(User user);
    Task<bool> UpdateAsync(User user);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UsernameExistsAsync(string username);

    // Extended methods for analytics
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<User, bool>> predicate);
    Task<List<User>> GetAllAsync();
    Task<List<User>> GetAllAsync(Expression<Func<User, bool>> predicate);
    IQueryable<User> GetQueryable();
}