using AscendDev.Core.DTOs.Courses;

namespace AscendDev.Core.Interfaces.Services;

public interface ILessonService
{
    // Read operations
    Task<List<LessonResponse>> GetLessonsByCourseId(string courseId);
    Task<LessonResponse> GetLessonById(string lessonId);
    Task<LessonResponse?> GetLessonBySlug(string slug);
    Task<List<LessonResponse>?> GetLessonsByStatus(string status);
    Task<List<LessonResponse>?> GetPublishedLessons();
    Task<List<LessonResponse>?> GetLessonsByCreator(string creatorId);
    Task<List<LessonResponse>?> GetLessonsByDifficulty(string difficulty);

    // CRUD operations
    Task<LessonResponse> CreateLesson(CreateLessonRequest request, string userId);
    Task<LessonResponse?> UpdateLesson(string lessonId, UpdateLessonRequest request, string userId);
    Task<bool> DeleteLesson(string lessonId, string userId);

    // Ordering operations
    Task<bool> ReorderLessons(string courseId, List<string> lessonIds, string userId);
    Task<List<LessonResponse>> GetOrderedLessonsByCourseId(string courseId);

    // Publishing operations
    Task<LessonResponse?> PublishLesson(string lessonId, string userId);
    Task<LessonResponse?> UnpublishLesson(string lessonId, string userId);
    Task<LessonResponse?> PreviewLesson(string lessonId, string userId);

    // Validation operations
    Task<LessonResponse?> ValidateLesson(string lessonId, string userId);
    Task<List<string>> GetLessonValidationErrors(string lessonId);

    // Analytics operations
    Task<bool> IncrementLessonViews(string lessonId);
    Task<bool> TrackLessonCompletion(string lessonId, string userId, double timeSpent);

    // Search and filtering
    Task<List<LessonResponse>?> SearchLessons(string searchTerm, string? courseId = null, string? difficulty = null, List<string>? tags = null);
    Task<List<LessonResponse>?> GetPaginatedLessons(int page, int pageSize, string? courseId = null, string? sortBy = null, bool ascending = true);

    // Prerequisites
    Task<List<LessonResponse>?> GetLessonPrerequisites(string lessonId);
    Task<bool> CanAccessLesson(string lessonId, string userId);
}