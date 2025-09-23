using AscendDev.Core.DTOs.Social;
using AscendDev.Core.Models.Social;

namespace AscendDev.Core.Interfaces.Services;

public interface ICodeReviewService
{
    Task<CodeReviewResponse?> GetByIdAsync(Guid id);
    Task<IEnumerable<CodeReviewResponse>> GetByLessonIdAsync(string lessonId, int page = 1, int pageSize = 20);
    Task<IEnumerable<CodeReviewResponse>> GetByReviewerIdAsync(Guid reviewerId, int page = 1, int pageSize = 20);
    Task<IEnumerable<CodeReviewResponse>> GetByRevieweeIdAsync(Guid revieweeId, int page = 1, int pageSize = 20);
    Task<IEnumerable<CodeReviewResponse>> GetByStatusAsync(CodeReviewStatus status, int page = 1, int pageSize = 20);
    Task<CodeReviewResponse> CreateAsync(CreateCodeReviewRequest request, Guid reviewerId);
    Task<CodeReviewResponse> UpdateAsync(Guid id, UpdateCodeReviewRequest request, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid userId);
    Task<int> GetTotalCountByLessonIdAsync(string lessonId);
    Task<int> GetTotalCountByReviewerIdAsync(Guid reviewerId);
    Task<int> GetTotalCountByRevieweeIdAsync(Guid revieweeId);
    Task<IEnumerable<CodeReviewResponse>> GetPendingReviewsAsync(int page = 1, int pageSize = 20);
    Task<CodeReviewResponse?> GetBySubmissionAndReviewerAsync(int submissionId, Guid reviewerId);
    Task<IEnumerable<CodeReviewResponse>> GetBySubmissionIdAsync(int submissionId);
}