using AscendDev.Core.DTOs.Social;

namespace AscendDev.Core.Interfaces.Services;

public interface ICodeReviewCommentService
{
    Task<CodeReviewCommentResponse?> GetByIdAsync(Guid id);
    Task<IEnumerable<CodeReviewCommentResponse>> GetByCodeReviewIdAsync(Guid codeReviewId);
    Task<IEnumerable<CodeReviewCommentResponse>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<CodeReviewCommentResponse> CreateAsync(Guid codeReviewId, CreateCodeReviewCommentRequest request, Guid userId);
    Task<CodeReviewCommentResponse> UpdateAsync(Guid id, UpdateCodeReviewCommentRequest request, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid userId);
    Task<IEnumerable<CodeReviewCommentResponse>> GetByLineNumberAsync(Guid codeReviewId, int lineNumber);
    Task<IEnumerable<CodeReviewCommentResponse>> GetUnresolvedByCodeReviewIdAsync(Guid codeReviewId);
    Task<int> GetTotalCountByCodeReviewIdAsync(Guid codeReviewId);
}