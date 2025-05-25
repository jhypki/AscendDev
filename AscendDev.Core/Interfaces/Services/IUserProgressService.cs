using AscendDev.Core.Models.Courses;

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
    Task<UserProgress> MarkLessonAsCompletedAsync(Guid userId, string lessonId, string codeSolution);

    /// <summary>
    /// Get the count of completed lessons for a user in a course
    /// </summary>
    Task<int> GetCompletedLessonCountForCourseAsync(Guid userId, string courseId);

    /// <summary>
    /// Get the completion percentage for a user in a course
    /// </summary>
    Task<double> GetCourseCompletionPercentageAsync(Guid userId, string courseId);
}