using AscendDev.Core.Models.Courses;
using AscendDev.Core.DTOs.Dashboard;

namespace AscendDev.Core.Interfaces.Services;

public interface IUserProgressService
{
    /// <summary>
    /// Get all progress entries for a specific user
    /// </summary>
    Task<List<UserProgress>> GetUserProgressAsync(Guid userId);

    /// <summary>
    /// Get all users who have completed a specific lesson
    /// </summary>
    Task<List<UserProgress>> GetLessonProgressAsync(string lessonId);

    /// <summary>
    /// Check if a user has completed a lesson
    /// </summary>
    Task<bool> HasUserCompletedLessonAsync(Guid userId, string lessonId);

    /// <summary>
    /// Mark a lesson as completed for a user
    /// </summary>
    Task<UserProgress> MarkLessonAsCompletedAsync(Guid userId, string lessonId, int submissionId);

    /// <summary>
    /// Get the count of completed lessons for a user in a course
    /// </summary>
    Task<int> GetCompletedLessonCountForCourseAsync(Guid userId, string courseId);

    /// <summary>
    /// Get the completion percentage for a user in a course
    /// </summary>
    Task<double> GetCourseCompletionPercentageAsync(Guid userId, string courseId);

    /// <summary>
    /// Get dashboard statistics for a user
    /// </summary>
    Task<DashboardStatsResponse> GetUserDashboardStatsAsync(Guid userId);

    /// <summary>
    /// Get user progress for all enrolled courses
    /// </summary>
    Task<List<UserProgressResponse>> GetUserCoursesProgressAsync(Guid userId);

    /// <summary>
    /// Get learning streak data for a user
    /// </summary>
    Task<List<LearningStreakResponse>> GetUserLearningStreakAsync(Guid userId, int days = 30);

    /// <summary>
    /// Get total completed lessons count for a user
    /// </summary>
    Task<int> GetTotalCompletedLessonsAsync(Guid userId);

    /// <summary>
    /// Get user's learning streak in days
    /// </summary>
    Task<int> GetUserStreakDaysAsync(Guid userId);

    /// <summary>
    /// Get recent activity for a user
    /// </summary>
    Task<List<RecentActivityResponse>> GetUserRecentActivityAsync(Guid userId, int limit = 10);
}