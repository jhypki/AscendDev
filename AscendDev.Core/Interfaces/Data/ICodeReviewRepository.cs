using AscendDev.Core.Models.Social;

namespace AscendDev.Core.Interfaces.Data;

public interface ICodeReviewRepository
{
    Task<CodeReview?> GetByIdAsync(Guid id);
    Task<IEnumerable<CodeReview>> GetByLessonIdAsync(string lessonId, int page = 1, int pageSize = 20);
    Task<IEnumerable<CodeReview>> GetByReviewerIdAsync(Guid reviewerId, int page = 1, int pageSize = 20);
    Task<IEnumerable<CodeReview>> GetByRevieweeIdAsync(Guid revieweeId, int page = 1, int pageSize = 20);
    Task<IEnumerable<CodeReview>> GetByStatusAsync(CodeReviewStatus status, int page = 1, int pageSize = 20);
    Task<CodeReview> CreateAsync(CodeReview codeReview);
    Task<CodeReview> UpdateAsync(CodeReview codeReview);
    Task<bool> DeleteAsync(Guid id);
    Task<int> GetTotalCountByLessonIdAsync(string lessonId);
    Task<int> GetTotalCountByReviewerIdAsync(Guid reviewerId);
    Task<int> GetTotalCountByRevieweeIdAsync(Guid revieweeId);
    Task<IEnumerable<CodeReview>> GetPendingReviewsAsync(int page = 1, int pageSize = 20);
    Task<CodeReview?> GetBySubmissionAndReviewerAsync(int submissionId, Guid reviewerId);
    Task<IEnumerable<CodeReview>> GetBySubmissionIdAsync(int submissionId);
}