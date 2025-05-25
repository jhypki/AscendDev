using AscendDev.Core.Models.Courses;

namespace AscendDev.Core.Interfaces.Data;

public interface IUserProgressRepository
{
    /// <summary>
    /// Get all progress entries for a specific user
    /// </summary>
    Task<List<UserProgress>> GetByUserId(Guid userId);

    /// <summary>
    /// Get all users who have completed a specific lesson
    /// </summary>
    Task<List<UserProgress>> GetByLessonId(string lessonId);

    /// <summary>
    /// Get a specific progress entry for a user and lesson
    /// </summary>
    Task<UserProgress?> GetByUserAndLessonId(Guid userId, string lessonId);

    /// <summary>
    /// Create a new progress entry
    /// </summary>
    Task<UserProgress> Create(UserProgress progress);

    /// <summary>
    /// Update an existing progress entry
    /// </summary>
    Task<bool> Update(UserProgress progress);

    /// <summary>
    /// Delete a progress entry
    /// </summary>
    Task<bool> Delete(int id);

    /// <summary>
    /// Check if a user has completed a lesson
    /// </summary>
    Task<bool> HasUserCompletedLesson(Guid userId, string lessonId);

    /// <summary>
    /// Get the count of completed lessons for a user in a course
    /// </summary>
    Task<int> GetCompletedLessonCountForCourse(Guid userId, string courseId);
}