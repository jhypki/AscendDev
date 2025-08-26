using AscendDev.Core.Models.Social;

namespace AscendDev.Core.Interfaces.Data;

public interface IDiscussionReplyRepository
{
    Task<DiscussionReply?> GetByIdAsync(Guid id);
    Task<IEnumerable<DiscussionReply>> GetByDiscussionIdAsync(Guid discussionId);
    Task<IEnumerable<DiscussionReply>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<DiscussionReply> CreateAsync(DiscussionReply reply);
    Task<DiscussionReply> UpdateAsync(DiscussionReply reply);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<DiscussionReply>> GetChildRepliesAsync(Guid parentReplyId);
    Task<int> GetTotalCountByDiscussionIdAsync(Guid discussionId);
    Task<DiscussionReply?> GetSolutionByDiscussionIdAsync(Guid discussionId);
}