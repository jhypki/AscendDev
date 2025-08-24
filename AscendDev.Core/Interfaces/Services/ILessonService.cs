using AscendDev.Core.DTOs.Courses;
using AscendDev.Core.Models.Courses;

namespace AscendDev.Core.Interfaces.Services;

public interface ILessonService
{
    // Read operations
    Task<List<LessonResponse>> GetLessonsByCourseId(string courseId);
    Task<LessonResponse> GetLessonById(string lessonId);
    Task<List<LessonResponse>> GetLessonsByCourseIdOrdered(string courseId);
    Task<LessonResponse?> GetNextLesson(string courseId, int currentOrder);
    Task<LessonResponse?> GetPreviousLesson(string courseId, int currentOrder);
    Task<List<LessonResponse>> GetLessonsByStatus(string status);

    // Write operations
    Task<LessonResponse> CreateLesson(Lesson lesson);
    Task<LessonResponse> UpdateLesson(Lesson lesson);
    Task<bool> DeleteLesson(string lessonId);
    Task<bool> UpdateLessonStatus(string lessonId, string status);
    Task<bool> UpdateLessonOrder(string lessonId, int newOrder);
    Task<bool> ReorderLessons(string courseId, List<string> lessonIds);

    // Preview and validation operations
    Task<LessonResponse?> PreviewLesson(string lessonId);
    Task<bool> ValidateLesson(Lesson lesson);
    Task<List<string>> GetLessonValidationErrors(Lesson lesson);
}