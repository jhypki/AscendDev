using AscendDev.Core.Models.Courses;

namespace AscendDev.Core.Interfaces.Data;

public interface ISubmissionRepository
{
    Task<Submission?> GetByIdAsync(int id);
    Task<IEnumerable<Submission>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Submission>> GetByLessonIdAsync(string lessonId);
    Task<Submission?> GetLatestByUserAndLessonAsync(Guid userId, string lessonId);
    Task<IEnumerable<Submission>> GetPublicPassedSubmissionsAsync(string lessonId, int limit = 50);
    Task<IEnumerable<Submission>> GetUserPublicPassedSubmissionsAsync(Guid userId, int limit = 50);
    Task<int> CreateAsync(Submission submission);
    Task UpdateAsync(Submission submission);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<int> GetSubmissionCountByUserAsync(Guid userId);
    Task<int> GetPassedSubmissionCountByUserAsync(Guid userId);
    Task<IEnumerable<Submission>> GetSubmissionsForReviewAsync(string lessonId, int limit = 50);
    Task<Submission?> GetSubmissionForReviewAsync(int submissionId);
}