using AscendDev.Core.DTOs.Courses;
using AscendDev.Core.DTOs.UserProfile;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.Auth;

namespace AscendDev.Services.Services;

public class UserSettingsService : IUserSettingsService
{
    private readonly IUserSettingsRepository _userSettingsRepository;

    public UserSettingsService(IUserSettingsRepository userSettingsRepository)
    {
        _userSettingsRepository = userSettingsRepository;
    }

    public async Task<UserSettingsResponse?> GetUserSettingsAsync(Guid userId)
    {
        var settings = await _userSettingsRepository.GetByUserIdAsync(userId);
        if (settings == null) return null;

        return new UserSettingsResponse
        {
            Id = settings.Id,
            UserId = settings.UserId,
            PublicSubmissions = settings.PublicSubmissions,
            ShowProfile = settings.ShowProfile,
            EmailOnCodeReview = settings.EmailOnCodeReview,
            EmailOnDiscussionReply = settings.EmailOnDiscussionReply,
            CreatedAt = settings.CreatedAt,
            UpdatedAt = settings.UpdatedAt
        };
    }

    public async Task<UserSettingsResponse> GetOrCreateUserSettingsAsync(Guid userId)
    {
        var settings = await _userSettingsRepository.GetOrCreateDefaultAsync(userId);

        return new UserSettingsResponse
        {
            Id = settings.Id,
            UserId = settings.UserId,
            PublicSubmissions = settings.PublicSubmissions,
            ShowProfile = settings.ShowProfile,
            EmailOnCodeReview = settings.EmailOnCodeReview,
            EmailOnDiscussionReply = settings.EmailOnDiscussionReply,
            CreatedAt = settings.CreatedAt,
            UpdatedAt = settings.UpdatedAt
        };
    }

    public async Task<UserSettingsResponse> UpdateUserSettingsAsync(Guid userId, UpdateUserSettingsRequest request)
    {
        var settings = await _userSettingsRepository.GetOrCreateDefaultAsync(userId);

        settings.PublicSubmissions = request.PublicSubmissions;
        settings.ShowProfile = request.ShowProfile;
        settings.EmailOnCodeReview = request.EmailOnCodeReview;
        settings.EmailOnDiscussionReply = request.EmailOnDiscussionReply;
        settings.UpdatedAt = DateTime.UtcNow;

        await _userSettingsRepository.UpdateAsync(settings);

        return new UserSettingsResponse
        {
            Id = settings.Id,
            UserId = settings.UserId,
            PublicSubmissions = settings.PublicSubmissions,
            ShowProfile = settings.ShowProfile,
            EmailOnCodeReview = settings.EmailOnCodeReview,
            EmailOnDiscussionReply = settings.EmailOnDiscussionReply,
            CreatedAt = settings.CreatedAt,
            UpdatedAt = settings.UpdatedAt
        };
    }

    public async Task DeleteUserSettingsAsync(Guid userId)
    {
        var settings = await _userSettingsRepository.GetByUserIdAsync(userId);
        if (settings != null)
        {
            await _userSettingsRepository.DeleteAsync(settings.Id);
        }
    }

    public async Task<bool> IsPublicSubmissionsEnabledAsync(Guid userId)
    {
        var settings = await _userSettingsRepository.GetByUserIdAsync(userId);
        return settings?.PublicSubmissions ?? false;
    }

    public async Task UpdatePrivacySettingsAsync(Guid userId, UpdatePrivacySettingsRequest request)
    {
        var settings = await _userSettingsRepository.GetOrCreateDefaultAsync(userId);

        // Map privacy settings to existing UserSettings properties
        // Note: This is a simplified mapping - in a real implementation you might want
        // to extend the UserSettings model to include these specific privacy fields
        settings.ShowProfile = request.IsProfilePublic;
        settings.UpdatedAt = DateTime.UtcNow;

        await _userSettingsRepository.UpdateAsync(settings);
    }
}