using AscendDev.Core.DTOs.UserProfile;

namespace AscendDev.Core.Interfaces.Services;

public interface IUserProfileService
{
    /// <summary>
    /// Get user profile by user ID
    /// </summary>
    /// <param name="userId">User ID to get profile for</param>
    /// <param name="requestingUserId">ID of user making the request (for privacy filtering)</param>
    /// <returns>User profile response</returns>
    Task<UserProfileResponse?> GetUserProfileAsync(Guid userId, Guid? requestingUserId = null);

    /// <summary>
    /// Get user profile by username
    /// </summary>
    /// <param name="username">Username to get profile for</param>
    /// <param name="requestingUserId">ID of user making the request (for privacy filtering)</param>
    /// <returns>User profile response</returns>
    Task<UserProfileResponse?> GetUserProfileByUsernameAsync(string username, Guid? requestingUserId = null);

    /// <summary>
    /// Update user profile
    /// </summary>
    /// <param name="userId">User ID to update</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated user profile</returns>
    Task<UserProfileResponse> UpdateUserProfileAsync(Guid userId, UpdateUserProfileRequest request);

    /// <summary>
    /// Get user activity feed
    /// </summary>
    /// <param name="request">Activity feed request</param>
    /// <param name="requestingUserId">ID of user making the request</param>
    /// <returns>Activity feed response</returns>
    Task<UserActivityFeedResponse> GetUserActivityFeedAsync(UserActivityFeedRequest request, Guid requestingUserId);

    /// <summary>
    /// Search user profiles
    /// </summary>
    /// <param name="request">Search request</param>
    /// <returns>Search results</returns>
    Task<UserProfileSearchResponse> SearchUserProfilesAsync(UserProfileSearchRequest request);

    /// <summary>
    /// Get user achievements
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of user achievements</returns>
    Task<List<UserAchievementResponse>> GetUserAchievementsAsync(Guid userId);

    /// <summary>
    /// Get user statistics
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User statistics</returns>
    Task<UserProfileStatistics> GetUserStatisticsAsync(Guid userId);

    /// <summary>
    /// Log user activity
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="activityType">Type of activity</param>
    /// <param name="description">Activity description</param>
    /// <param name="metadata">Additional metadata</param>
    /// <param name="relatedEntityId">Related entity ID</param>
    /// <param name="relatedEntityType">Related entity type</param>
    /// <returns>Task</returns>
    Task LogUserActivityAsync(
        Guid userId,
        string activityType,
        string description,
        Dictionary<string, object>? metadata = null,
        string? relatedEntityId = null,
        string? relatedEntityType = null);

    /// <summary>
    /// Check if user profile is public
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>True if profile is public</returns>
    Task<bool> IsProfilePublicAsync(Guid userId);
}