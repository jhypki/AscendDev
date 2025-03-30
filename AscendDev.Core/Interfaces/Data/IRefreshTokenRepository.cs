using AscendDev.Core.Entities.Auth;

namespace AscendDev.Core.Interfaces.Data;

public interface IRefreshTokenRepository
{
    Task SaveAsync(RefreshToken refreshToken);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task DeleteAsync(string token);
    Task DeleteByUserIdAsync(Guid userId);
    Task<bool> RevokeAsync(string token, string ip);
}