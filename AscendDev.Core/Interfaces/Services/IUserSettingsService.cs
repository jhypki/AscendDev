using AscendDev.Core.DTOs.Courses;
using AscendDev.Core.Models.Auth;

namespace AscendDev.Core.Interfaces.Services;

public interface IUserSettingsService
{
    Task<UserSettingsResponse?> GetUserSettingsAsync(Guid userId);
    Task<UserSettingsResponse> GetOrCreateUserSettingsAsync(Guid userId);
    Task<UserSettingsResponse> UpdateUserSettingsAsync(Guid userId, UpdateUserSettingsRequest request);
    Task DeleteUserSettingsAsync(Guid userId);
    Task<bool> IsPublicSubmissionsEnabledAsync(Guid userId);
}