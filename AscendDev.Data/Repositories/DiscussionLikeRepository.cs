using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Social;
using Dapper;

namespace AscendDev.Data.Repositories;

public class DiscussionLikeRepository : IDiscussionLikeRepository
{
    private readonly ISqlExecutor _sqlExecutor;

    public DiscussionLikeRepository(ISqlExecutor sqlExecutor)
    {
        _sqlExecutor = sqlExecutor;
    }

    public async Task<DiscussionLike?> GetByDiscussionAndUserAsync(Guid discussionId, Guid userId)
    {
        var sql = @"
            SELECT * FROM discussion_likes 
            WHERE discussion_id = @discussionId AND user_id = @userId";

        return await _sqlExecutor.QueryFirstOrDefaultAsync<DiscussionLike>(sql, new { discussionId, userId });
    }

    public async Task<DiscussionLike> CreateAsync(DiscussionLike like)
    {
        var sql = @"
            INSERT INTO discussion_likes (id, discussion_id, user_id, created_at)
            VALUES (@Id, @DiscussionId, @UserId, @CreatedAt)
            RETURNING *";

        return await _sqlExecutor.QueryFirstAsync<DiscussionLike>(sql, like);
    }

    public async Task<bool> DeleteAsync(Guid discussionId, Guid userId)
    {
        var sql = @"
            DELETE FROM discussion_likes 
            WHERE discussion_id = @discussionId AND user_id = @userId";

        var rowsAffected = await _sqlExecutor.ExecuteAsync(sql, new { discussionId, userId });
        return rowsAffected > 0;
    }

    public async Task<int> GetLikeCountAsync(Guid discussionId)
    {
        var sql = "SELECT COUNT(*) FROM discussion_likes WHERE discussion_id = @discussionId";
        return await _sqlExecutor.QueryFirstAsync<int>(sql, new { discussionId });
    }

    public async Task<bool> IsLikedByUserAsync(Guid discussionId, Guid userId)
    {
        var sql = @"
            SELECT COUNT(*) FROM discussion_likes 
            WHERE discussion_id = @discussionId AND user_id = @userId";

        var count = await _sqlExecutor.QueryFirstAsync<int>(sql, new { discussionId, userId });
        return count > 0;
    }
}