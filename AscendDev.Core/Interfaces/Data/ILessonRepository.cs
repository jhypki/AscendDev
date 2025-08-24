using AscendDev.Core.Models.Courses;

namespace AscendDev.Core.Interfaces.Data;

public interface ILessonRepository
{
    // Read operations
    Task<List<Lesson>?> GetByCourseId(string courseId);
    Task<Lesson?> GetById(string lessonId);
    Task<Lesson?> GetBySlug(string slug);
    Task<List<Lesson>?> GetByStatus(string status);
    Task<List<Lesson>?> GetPublished();
    Task<List<Lesson>?> GetByCreator(string creatorId);
    Task<List<Lesson>?> GetByDifficulty(string difficulty);

    // CRUD operations
    Task<Lesson> Create(Lesson lesson);
    Task<Lesson?> Update(string lessonId, Lesson lesson);
    Task<bool> Delete(string lessonId);
    Task<bool> SoftDelete(string lessonId);

    // Ordering operations
    Task<bool> ReorderLessons(string courseId, List<string> lessonIds);
    Task<List<Lesson>?> GetOrderedByCourseId(string courseId);
    Task<int> GetNextOrderNumber(string courseId);

    // Publishing operations
    Task<bool> Publish(string lessonId, string userId);
    Task<bool> Unpublish(string lessonId, string userId);

    // Validation operations
    Task<bool> Validate(string lessonId, string userId, List<string> validationErrors);
    Task<bool> MarkAsValidated(string lessonId, string userId);

    // Analytics operations
    Task<bool> IncrementViewCount(string lessonId);
    Task<bool> IncrementCompletionCount(string lessonId);
    Task<bool> UpdateAverageTimeSpent(string lessonId, double timeSpent);

    // Search and filtering
    Task<List<Lesson>?> Search(string searchTerm, string? courseId = null, string? difficulty = null, List<string>? tags = null);
    Task<List<Lesson>?> GetPaginated(int page, int pageSize, string? courseId = null, string? sortBy = null, bool ascending = true);

    // Prerequisites
    Task<List<Lesson>?> GetPrerequisites(string lessonId);
    Task<bool> HasCompletedPrerequisites(string lessonId, string userId);
}