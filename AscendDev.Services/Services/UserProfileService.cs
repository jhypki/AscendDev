using AscendDev.Core.DTOs.UserProfile;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Auth;
using AscendDev.Core.Models.Admin;
using Microsoft.Extensions.Logging;

namespace AscendDev.Services.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserSettingsRepository _userSettingsRepository;
    private readonly IUserManagementRepository _userManagementRepository;
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IDiscussionRepository _discussionRepository;
    private readonly ILogger<UserProfileService> _logger;

    public UserProfileService(
        IUserRepository userRepository,
        IUserSettingsRepository userSettingsRepository,
        IUserManagementRepository userManagementRepository,
        ISubmissionRepository submissionRepository,
        IDiscussionRepository discussionRepository,
        ILogger<UserProfileService> logger)
    {
        _userRepository = userRepository;
        _userSettingsRepository = userSettingsRepository;
        _userManagementRepository = userManagementRepository;
        _submissionRepository = submissionRepository;
        _discussionRepository = discussionRepository;
        _logger = logger;
    }

    public async Task<UserProfileResponse?> GetUserProfileAsync(Guid userId, Guid? requestingUserId = null)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var userRoles = await _userManagementRepository.GetUserRolesAsync(userId);
            var settings = await _userSettingsRepository.GetByUserIdAsync(userId);

            // Check if profile should be shown
            if (settings != null && !settings.ShowProfile && requestingUserId != userId)
            {
                return null;
            }

            var statistics = await GetUserStatisticsAsync(userId);
            var recentActivity = await GetRecentUserActivityAsync(userId, 10);
            var achievements = await GetUserAchievementsAsync(userId);

            var profile = new UserProfileResponse
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Bio = user.Bio,
                ProfilePictureUrl = user.ProfilePictureUrl,
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin,
                IsEmailVerified = user.IsEmailVerified,
                Provider = user.Provider,
                Roles = userRoles,
                Statistics = statistics,
                RecentActivity = recentActivity,
                Achievements = achievements
            };

            // Only include email and settings for own profile
            if (requestingUserId == userId)
            {
                profile.Email = user.Email;
                if (settings != null)
                {
                    profile.Settings = new UserProfileSettings
                    {
                        PublicSubmissions = settings.PublicSubmissions,
                        ShowProfile = settings.ShowProfile,
                        EmailOnCodeReview = settings.EmailOnCodeReview,
                        EmailOnDiscussionReply = settings.EmailOnDiscussionReply
                    };
                }
            }

            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile for {UserId}", userId);
            throw;
        }
    }

    public async Task<UserProfileResponse?> GetUserProfileByUsernameAsync(string username, Guid? requestingUserId = null)
    {
        try
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
            {
                return null;
            }

            return await GetUserProfileAsync(user.Id, requestingUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile by username {Username}", username);
            throw;
        }
    }

    public async Task<UserProfileResponse> UpdateUserProfileAsync(Guid userId, UpdateUserProfileRequest request)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found", nameof(userId));
            }

            // Update user fields
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Bio = request.Bio;
            user.ProfilePictureUrl = request.ProfilePictureUrl;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            // Log activity
            await LogUserActivityAsync(userId, "profile_updated", "Updated profile information");

            // Return updated profile
            var updatedProfile = await GetUserProfileAsync(userId, userId);
            return updatedProfile!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile for {UserId}", userId);
            throw;
        }
    }

    public async Task<UserActivityFeedResponse> GetUserActivityFeedAsync(UserActivityFeedRequest request, Guid requestingUserId)
    {
        try
        {
            var targetUserId = request.UserId ?? requestingUserId;

            // Check if we can view this user's activity
            if (targetUserId != requestingUserId)
            {
                var isPublic = await IsProfilePublicAsync(targetUserId);
                if (!isPublic)
                {
                    return new UserActivityFeedResponse
                    {
                        Activities = new List<UserActivityResponse>(),
                        TotalCount = 0,
                        HasMore = false
                    };
                }
            }

            var activities = await _userManagementRepository.GetUserActivityAsync(targetUserId, request.Limit + 1);

            var hasMore = activities.Count > request.Limit;
            if (hasMore)
            {
                activities = activities.Take(request.Limit).ToList();
            }

            var activityResponses = activities.Select(a => new UserActivityResponse
            {
                Id = a.Id,
                Type = a.ActivityType,
                Description = a.ActivityDescription ?? "",
                Metadata = a.Metadata != null ?
                    System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(a.Metadata.ToString()!) :
                    null,
                CreatedAt = a.CreatedAt
            }).ToList();

            return new UserActivityFeedResponse
            {
                Activities = activityResponses,
                TotalCount = activities.Count,
                HasMore = hasMore,
                NextOffset = hasMore ? request.Offset + request.Limit : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user activity feed");
            throw;
        }
    }

    public async Task<UserProfileSearchResponse> SearchUserProfilesAsync(UserProfileSearchRequest request)
    {
        try
        {
            // Simple search implementation - get all users and filter by username/name
            var allUsers = await _userRepository.GetAllAsync();

            var filteredUsers = allUsers.Where(u =>
                string.IsNullOrEmpty(request.Query) ||
                u.Username.Contains(request.Query, StringComparison.OrdinalIgnoreCase) ||
                (u.FirstName != null && u.FirstName.Contains(request.Query, StringComparison.OrdinalIgnoreCase)) ||
                (u.LastName != null && u.LastName.Contains(request.Query, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            var hasMore = filteredUsers.Count > request.Limit;
            if (hasMore)
            {
                filteredUsers = filteredUsers.Take(request.Limit).ToList();
            }

            var profiles = new List<UserProfileSummary>();

            foreach (var user in filteredUsers)
            {
                // Check if profile is public
                var settings = await _userSettingsRepository.GetByUserIdAsync(user.Id);
                if (settings?.ShowProfile == false)
                    continue;

                // Get basic statistics for the summary
                var submissions = await _submissionRepository.GetByUserIdAsync(user.Id);
                var successfulSubmissions = submissions.Where(s => s.Passed).ToList();
                var totalPoints = successfulSubmissions.Count * 10; // Simple point calculation
                var lessonsCompleted = successfulSubmissions.Select(s => s.LessonId).Distinct().Count();

                profiles.Add(new UserProfileSummary
                {
                    Id = user.Id,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Bio = user.Bio,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    TotalPoints = totalPoints,
                    LessonsCompleted = lessonsCompleted,
                    CoursesCompleted = lessonsCompleted / 10, // Estimate
                    CurrentStreak = 0, // Would need proper calculation
                    JoinedDate = user.CreatedAt,
                    LastActivityDate = user.LastLogin
                });
            }

            return new UserProfileSearchResponse
            {
                Profiles = profiles,
                TotalCount = profiles.Count,
                HasMore = hasMore
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching user profiles");
            throw;
        }
    }

    public async Task<List<UserAchievementResponse>> GetUserAchievementsAsync(Guid userId)
    {
        try
        {
            // This would need to be implemented with achievement repository
            // For now, return empty list
            return new List<UserAchievementResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user achievements for {UserId}", userId);
            throw;
        }
    }

    public async Task<UserProfileStatistics> GetUserStatisticsAsync(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found", nameof(userId));
            }

            // Get actual statistics from database
            var submissions = await _submissionRepository.GetByUserIdAsync(userId);
            var discussions = await _discussionRepository.GetByUserIdAsync(userId);

            // Calculate statistics
            var successfulSubmissions = submissions.Where(s => s.Passed).ToList();
            var totalPoints = successfulSubmissions.Count * 10; // Simple point calculation

            // Get unique lessons completed
            var completedLessons = successfulSubmissions
                .Select(s => s.LessonId)
                .Distinct()
                .Count();

            // For courses completed, estimate based on lessons (assuming 10 lessons per course on average)
            var estimatedCoursesCompleted = completedLessons / 10;

            // Calculate streaks (simplified - would need proper date-based calculation)
            var recentActivity = await _userManagementRepository.GetUserActivityAsync(userId, 30);
            // Simple streak calculation - for now just return 0
            var currentStreak = 0;
            var longestStreak = 0;

            return new UserProfileStatistics
            {
                TotalPoints = totalPoints,
                LessonsCompleted = completedLessons,
                CoursesCompleted = estimatedCoursesCompleted,
                CurrentStreak = currentStreak,
                LongestStreak = longestStreak,
                TotalCodeReviews = 0, // Would need code review repository
                TotalDiscussions = discussions.Count(),
                LastActivityDate = user.LastLogin,
                JoinedDate = user.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user statistics for {UserId}", userId);
            throw;
        }
    }

    public async Task LogUserActivityAsync(
        Guid userId,
        string activityType,
        string description,
        Dictionary<string, object>? metadata = null,
        string? relatedEntityId = null,
        string? relatedEntityType = null)
    {
        try
        {
            var activityLog = new UserActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ActivityType = activityType,
                ActivityDescription = description,
                Metadata = metadata,
                CreatedAt = DateTime.UtcNow
            };

            await _userManagementRepository.LogUserActivityAsync(activityLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging user activity for {UserId}", userId);
            // Don't throw - activity logging shouldn't break the main flow
        }
    }

    private (int currentStreak, int longestStreak) CalculateStreaks(List<UserActivityLog> activities)
    {
        if (!activities.Any())
            return (0, 0);

        // Simple implementation - count consecutive days with activity
        var activityDates = activities
            .Select(a => a.CreatedAt.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        var currentStreak = 0;
        var longestStreak = 0;
        var tempStreak = 0;
        var previousDate = DateTime.MinValue;

        foreach (var date in activityDates)
        {
            if (previousDate == DateTime.MinValue || (previousDate - date).Days == 1)
            {
                tempStreak++;
                if (previousDate == DateTime.MinValue || date == DateTime.Today || date == DateTime.Today.AddDays(-1))
                {
                    currentStreak = tempStreak;
                }
            }
            else
            {
                longestStreak = Math.Max(longestStreak, tempStreak);
                tempStreak = 1;
                if (date == DateTime.Today || date == DateTime.Today.AddDays(-1))
                {
                    currentStreak = tempStreak;
                }
            }
            previousDate = date;
        }

        longestStreak = Math.Max(longestStreak, tempStreak);
        return (currentStreak, longestStreak);
    }

    public async Task<bool> IsProfilePublicAsync(Guid userId)
    {
        try
        {
            var settings = await _userSettingsRepository.GetByUserIdAsync(userId);
            return settings?.ShowProfile ?? true; // Default to public if no settings
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if profile is public for {UserId}", userId);
            return false; // Default to private on error
        }
    }

    private async Task<List<UserActivityResponse>> GetRecentUserActivityAsync(Guid userId, int limit)
    {
        try
        {
            var activities = await _userManagementRepository.GetUserActivityAsync(userId, limit);
            return activities.Select(a => new UserActivityResponse
            {
                Id = a.Id,
                Type = a.ActivityType,
                Description = a.ActivityDescription ?? "",
                Metadata = !string.IsNullOrEmpty(a.Metadata) ?
                    System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(a.Metadata) :
                    null,
                CreatedAt = a.CreatedAt
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent user activity for {UserId}", userId);
            return new List<UserActivityResponse>();
        }
    }
}