using AscendDev.Core.DTOs.Social;

namespace AscendDev.Core.Interfaces.Services;

public interface IDiscussionReplyService
{
    Task<DiscussionReplyResponse?> GetByIdAsync(Guid id);
    Task<IEnumerable<DiscussionReplyResponse>> GetByDiscussionIdAsync(Guid discussionId);
    Task<IEnumerable<DiscussionReplyResponse>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<DiscussionReplyResponse> CreateAsync(Guid discussionId, CreateDiscussionReplyRequest request, Guid userId);
    Task<DiscussionReplyResponse> UpdateAsync(Guid id, UpdateDiscussionReplyRequest request, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid userId);
    Task<IEnumerable<DiscussionReplyResponse>> GetChildRepliesAsync(Guid parentReplyId);
    Task<int> GetTotalCountByDiscussionIdAsync(Guid discussionId);
    Task<DiscussionReplyResponse?> GetSolutionByDiscussionIdAsync(Guid discussionId);
}