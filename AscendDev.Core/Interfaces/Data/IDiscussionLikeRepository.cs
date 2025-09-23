using AscendDev.Core.Models.Social;

namespace AscendDev.Core.Interfaces.Data;

public interface IDiscussionLikeRepository
{
    Task<DiscussionLike?> GetByDiscussionAndUserAsync(Guid discussionId, Guid userId);
    Task<DiscussionLike> CreateAsync(DiscussionLike like);
    Task<bool> DeleteAsync(Guid discussionId, Guid userId);
    Task<int> GetLikeCountAsync(Guid discussionId);
    Task<bool> IsLikedByUserAsync(Guid discussionId, Guid userId);
}