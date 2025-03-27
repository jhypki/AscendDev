using ElearningPlatform.Core.Entities.Auth;

namespace ElearningPlatform.Core.Interfaces.Data;

public interface IRefreshTokenRepository
{
    Task SaveAsync(RefreshToken refreshToken);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task DeleteAsync(string token);
    Task DeleteByUserIdAsync(Guid userId);
}