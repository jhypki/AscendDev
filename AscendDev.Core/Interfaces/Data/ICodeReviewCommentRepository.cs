using AscendDev.Core.Models.Social;

namespace AscendDev.Core.Interfaces.Data;

public interface ICodeReviewCommentRepository
{
    Task<CodeReviewComment?> GetByIdAsync(Guid id);
    Task<IEnumerable<CodeReviewComment>> GetByCodeReviewIdAsync(Guid codeReviewId);
    Task<IEnumerable<CodeReviewComment>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<CodeReviewComment> CreateAsync(CodeReviewComment comment);
    Task<CodeReviewComment> UpdateAsync(CodeReviewComment comment);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<CodeReviewComment>> GetByLineNumberAsync(Guid codeReviewId, int lineNumber);
    Task<IEnumerable<CodeReviewComment>> GetUnresolvedByCodeReviewIdAsync(Guid codeReviewId);
    Task<int> GetTotalCountByCodeReviewIdAsync(Guid codeReviewId);
    Task<IEnumerable<CodeReviewComment>> GetRepliesByParentIdAsync(Guid parentCommentId);
}