using AscendDev.Core.Models.Courses;

namespace AscendDev.Core.Interfaces.Data;

public interface ILessonRepository
{
    // Read operations
    Task<List<Lesson>?> GetByCourseId(string courseId);
    Task<Lesson?> GetById(string lessonId);
    Task<Lesson?> GetBySlug(string slug);
    Task<List<Lesson>?> GetByStatus(string status);
    Task<List<Lesson>?> GetByCourseIdOrdered(string courseId);
    Task<Lesson?> GetNextLesson(string courseId, int currentOrder);
    Task<Lesson?> GetPreviousLesson(string courseId, int currentOrder);

    // Write operations
    Task<Lesson> Create(Lesson lesson);
    Task<Lesson> Update(Lesson lesson);
    Task<bool> Delete(string lessonId);
    Task<bool> UpdateStatus(string lessonId, string status);
    Task<bool> UpdateOrder(string lessonId, int newOrder);
    Task<bool> ReorderLessons(string courseId, List<string> lessonIds);

    // Validation operations
    Task<bool> ValidateLesson(Lesson lesson);
    Task<List<string>> GetValidationErrors(Lesson lesson);
}