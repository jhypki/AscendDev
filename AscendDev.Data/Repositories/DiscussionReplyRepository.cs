using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Social;
using Dapper;

namespace AscendDev.Data.Repositories;

public class DiscussionReplyRepository : IDiscussionReplyRepository
{
    private readonly ISqlExecutor _sqlExecutor;

    public DiscussionReplyRepository(ISqlExecutor sqlExecutor)
    {
        _sqlExecutor = sqlExecutor;
    }

    public async Task<DiscussionReply?> GetByIdAsync(Guid id)
    {
        var sql = @"
            SELECT dr.*, u.id, u.username, u.email, u.first_name, u.last_name, u.profile_picture_url
            FROM discussion_replies dr
            INNER JOIN users u ON dr.user_id = u.id
            WHERE dr.id = @id";

        var replies = await _sqlExecutor.QueryAsync<DiscussionReply, dynamic, DiscussionReply>(
            sql,
            (reply, user) =>
            {
                reply.User = new Core.Models.Auth.User
                {
                    Id = user.id,
                    Username = user.username,
                    Email = user.email,
                    FirstName = user.first_name,
                    LastName = user.last_name,
                    ProfilePictureUrl = user.profile_picture_url
                };
                return reply;
            },
            new { id },
            splitOn: "id");

        return replies.FirstOrDefault();
    }

    public async Task<IEnumerable<DiscussionReply>> GetByDiscussionIdAsync(Guid discussionId)
    {
        var sql = @"
            SELECT dr.*, u.id, u.username, u.email, u.first_name, u.last_name, u.profile_picture_url
            FROM discussion_replies dr
            INNER JOIN users u ON dr.user_id = u.id
            WHERE dr.discussion_id = @discussionId
            ORDER BY dr.created_at ASC";

        var replies = await _sqlExecutor.QueryAsync<DiscussionReply, dynamic, DiscussionReply>(
            sql,
            (reply, user) =>
            {
                reply.User = new Core.Models.Auth.User
                {
                    Id = user.id,
                    Username = user.username,
                    Email = user.email,
                    FirstName = user.first_name,
                    LastName = user.last_name,
                    ProfilePictureUrl = user.profile_picture_url
                };
                return reply;
            },
            new { discussionId },
            splitOn: "id");

        return replies;
    }

    public async Task<IEnumerable<DiscussionReply>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var offset = (page - 1) * pageSize;
        var sql = @"
            SELECT dr.*, u.id, u.username, u.email, u.first_name, u.last_name, u.profile_picture_url
            FROM discussion_replies dr
            INNER JOIN users u ON dr.user_id = u.id
            WHERE dr.user_id = @userId
            ORDER BY dr.created_at DESC
            LIMIT @pageSize OFFSET @offset";

        var replies = await _sqlExecutor.QueryAsync<DiscussionReply, dynamic, DiscussionReply>(
            sql,
            (reply, user) =>
            {
                reply.User = new Core.Models.Auth.User
                {
                    Id = user.id,
                    Username = user.username,
                    Email = user.email,
                    FirstName = user.first_name,
                    LastName = user.last_name,
                    ProfilePictureUrl = user.profile_picture_url
                };
                return reply;
            },
            new { userId, pageSize, offset },
            splitOn: "id");

        return replies;
    }

    public async Task<DiscussionReply> CreateAsync(DiscussionReply reply)
    {
        var sql = @"
            INSERT INTO discussion_replies (id, discussion_id, user_id, content, created_at, parent_reply_id, is_solution)
            VALUES (@Id, @DiscussionId, @UserId, @Content, @CreatedAt, @ParentReplyId, @IsSolution)
            RETURNING *";

        var result = await _sqlExecutor.QueryFirstAsync<DiscussionReply>(sql, reply);
        return result;
    }

    public async Task<DiscussionReply> UpdateAsync(DiscussionReply reply)
    {
        var sql = @"
            UPDATE discussion_replies 
            SET content = @Content, updated_at = @UpdatedAt, is_solution = @IsSolution
            WHERE id = @Id
            RETURNING *";

        var result = await _sqlExecutor.QueryFirstAsync<DiscussionReply>(sql, reply);
        return result;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var sql = "DELETE FROM discussion_replies WHERE id = @id";
        var rowsAffected = await _sqlExecutor.ExecuteAsync(sql, new { id });
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<DiscussionReply>> GetChildRepliesAsync(Guid parentReplyId)
    {
        var sql = @"
            SELECT dr.*, u.id, u.username, u.email, u.first_name, u.last_name, u.profile_picture_url
            FROM discussion_replies dr
            INNER JOIN users u ON dr.user_id = u.id
            WHERE dr.parent_reply_id = @parentReplyId
            ORDER BY dr.created_at ASC";

        var replies = await _sqlExecutor.QueryAsync<DiscussionReply, dynamic, DiscussionReply>(
            sql,
            (reply, user) =>
            {
                reply.User = new Core.Models.Auth.User
                {
                    Id = user.id,
                    Username = user.username,
                    Email = user.email,
                    FirstName = user.first_name,
                    LastName = user.last_name,
                    ProfilePictureUrl = user.profile_picture_url
                };
                return reply;
            },
            new { parentReplyId },
            splitOn: "id");

        return replies;
    }

    public async Task<int> GetTotalCountByDiscussionIdAsync(Guid discussionId)
    {
        var sql = "SELECT COUNT(*) FROM discussion_replies WHERE discussion_id = @discussionId";
        return await _sqlExecutor.QueryFirstAsync<int>(sql, new { discussionId });
    }

    public async Task<DiscussionReply?> GetSolutionByDiscussionIdAsync(Guid discussionId)
    {
        var sql = @"
            SELECT dr.*, u.id, u.username, u.email, u.first_name, u.last_name, u.profile_picture_url
            FROM discussion_replies dr
            INNER JOIN users u ON dr.user_id = u.id
            WHERE dr.discussion_id = @discussionId AND dr.is_solution = true
            LIMIT 1";

        var replies = await _sqlExecutor.QueryAsync<DiscussionReply, dynamic, DiscussionReply>(
            sql,
            (reply, user) =>
            {
                reply.User = new Core.Models.Auth.User
                {
                    Id = user.id,
                    Username = user.username,
                    Email = user.email,
                    FirstName = user.first_name,
                    LastName = user.last_name,
                    ProfilePictureUrl = user.profile_picture_url
                };
                return reply;
            },
            new { discussionId },
            splitOn: "id");

        return replies.FirstOrDefault();
    }
}