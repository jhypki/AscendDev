using AscendDev.Core.Models.Auth;

namespace AscendDev.Core.Interfaces.Data;

public interface IUserSettingsRepository
{
    Task<UserSettings?> GetByUserIdAsync(Guid userId);
    Task<UserSettings?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(UserSettings userSettings);
    Task UpdateAsync(UserSettings userSettings);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid userId);
    Task<UserSettings> GetOrCreateDefaultAsync(Guid userId);
}