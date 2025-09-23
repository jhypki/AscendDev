using AscendDev.Core.Models.Social;

namespace AscendDev.Core.Interfaces.Data;

public interface IDiscussionRepository
{
    Task<Discussion?> GetByIdAsync(Guid id);
    Task<IEnumerable<Discussion>> GetByLessonIdAsync(string lessonId, int page = 1, int pageSize = 20);
    Task<IEnumerable<Discussion>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<Discussion> CreateAsync(Discussion discussion);
    Task<Discussion> UpdateAsync(Discussion discussion);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> IncrementViewCountAsync(Guid id);
    Task<IEnumerable<Discussion>> GetPinnedByLessonIdAsync(string lessonId);
    Task<int> GetTotalCountByLessonIdAsync(string lessonId);
    Task<IEnumerable<Discussion>> SearchAsync(string searchTerm, string? lessonId = null, int page = 1, int pageSize = 20);

    // Course-level discussion methods
    Task<IEnumerable<Discussion>> GetByCourseIdAsync(string courseId, int page = 1, int pageSize = 20);
    Task<int> GetTotalCountByCourseIdAsync(string courseId);
    Task<IEnumerable<Discussion>> GetPinnedByCourseIdAsync(string courseId);
}