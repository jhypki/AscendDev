using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Social;
using Dapper;

namespace AscendDev.Data.Repositories;

public class CodeReviewCommentRepository : ICodeReviewCommentRepository
{
    private readonly ISqlExecutor _sqlExecutor;

    public CodeReviewCommentRepository(ISqlExecutor sqlExecutor)
    {
        _sqlExecutor = sqlExecutor;
    }

    public async Task<CodeReviewComment?> GetByIdAsync(Guid id)
    {
        var sql = @"
            SELECT crc.*, u.id, u.username, u.email, u.first_name, u.last_name, u.profile_picture_url
            FROM code_review_comments crc
            INNER JOIN users u ON crc.user_id = u.id
            WHERE crc.id = @id";

        var comments = await _sqlExecutor.QueryAsync<CodeReviewComment, dynamic, CodeReviewComment>(
            sql,
            (comment, user) =>
            {
                comment.User = new Core.Models.Auth.User
                {
                    Id = user.id,
                    Username = user.username,
                    Email = user.email,
                    FirstName = user.first_name,
                    LastName = user.last_name,
                    ProfilePictureUrl = user.profile_picture_url
                };
                return comment;
            },
            new { id },
            splitOn: "id");

        return comments.FirstOrDefault();
    }

    public async Task<IEnumerable<CodeReviewComment>> GetByCodeReviewIdAsync(Guid codeReviewId)
    {
        var sql = @"
            SELECT crc.*, u.id, u.username, u.email, u.first_name, u.last_name, u.profile_picture_url
            FROM code_review_comments crc
            INNER JOIN users u ON crc.user_id = u.id
            WHERE crc.code_review_id = @codeReviewId
            ORDER BY crc.line_number ASC, crc.created_at ASC";

        var comments = await _sqlExecutor.QueryAsync<CodeReviewComment, dynamic, CodeReviewComment>(
            sql,
            (comment, user) =>
            {
                comment.User = new Core.Models.Auth.User
                {
                    Id = user.id,
                    Username = user.username,
                    Email = user.email,
                    FirstName = user.first_name,
                    LastName = user.last_name,
                    ProfilePictureUrl = user.profile_picture_url
                };
                return comment;
            },
            new { codeReviewId },
            splitOn: "id");

        return comments;
    }

    public async Task<IEnumerable<CodeReviewComment>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var offset = (page - 1) * pageSize;
        var sql = @"
            SELECT crc.*, u.id, u.username, u.email, u.first_name, u.last_name, u.profile_picture_url
            FROM code_review_comments crc
            INNER JOIN users u ON crc.user_id = u.id
            WHERE crc.user_id = @userId
            ORDER BY crc.created_at DESC
            LIMIT @pageSize OFFSET @offset";

        var comments = await _sqlExecutor.QueryAsync<CodeReviewComment, dynamic, CodeReviewComment>(
            sql,
            (comment, user) =>
            {
                comment.User = new Core.Models.Auth.User
                {
                    Id = user.id,
                    Username = user.username,
                    Email = user.email,
                    FirstName = user.first_name,
                    LastName = user.last_name,
                    ProfilePictureUrl = user.profile_picture_url
                };
                return comment;
            },
            new { userId, pageSize, offset },
            splitOn: "id");

        return comments;
    }

    public async Task<CodeReviewComment> CreateAsync(CodeReviewComment comment)
    {
        var sql = @"
            INSERT INTO code_review_comments (id, code_review_id, user_id, line_number, content, created_at, is_resolved)
            VALUES (@Id, @CodeReviewId, @UserId, @LineNumber, @Content, @CreatedAt, @IsResolved)
            RETURNING *";

        var result = await _sqlExecutor.QueryFirstAsync<CodeReviewComment>(sql, comment);
        return result;
    }

    public async Task<CodeReviewComment> UpdateAsync(CodeReviewComment comment)
    {
        var sql = @"
            UPDATE code_review_comments 
            SET content = @Content, updated_at = @UpdatedAt, is_resolved = @IsResolved
            WHERE id = @Id
            RETURNING *";

        var result = await _sqlExecutor.QueryFirstAsync<CodeReviewComment>(sql, comment);
        return result;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var sql = "DELETE FROM code_review_comments WHERE id = @id";
        var rowsAffected = await _sqlExecutor.ExecuteAsync(sql, new { id });
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<CodeReviewComment>> GetByLineNumberAsync(Guid codeReviewId, int lineNumber)
    {
        var sql = @"
            SELECT crc.*, u.id, u.username, u.email, u.first_name, u.last_name, u.profile_picture_url
            FROM code_review_comments crc
            INNER JOIN users u ON crc.user_id = u.id
            WHERE crc.code_review_id = @codeReviewId AND crc.line_number = @lineNumber
            ORDER BY crc.created_at ASC";

        var comments = await _sqlExecutor.QueryAsync<CodeReviewComment, dynamic, CodeReviewComment>(
            sql,
            (comment, user) =>
            {
                comment.User = new Core.Models.Auth.User
                {
                    Id = user.id,
                    Username = user.username,
                    Email = user.email,
                    FirstName = user.first_name,
                    LastName = user.last_name,
                    ProfilePictureUrl = user.profile_picture_url
                };
                return comment;
            },
            new { codeReviewId, lineNumber },
            splitOn: "id");

        return comments;
    }

    public async Task<IEnumerable<CodeReviewComment>> GetUnresolvedByCodeReviewIdAsync(Guid codeReviewId)
    {
        var sql = @"
            SELECT crc.*, u.id, u.username, u.email, u.first_name, u.last_name, u.profile_picture_url
            FROM code_review_comments crc
            INNER JOIN users u ON crc.user_id = u.id
            WHERE crc.code_review_id = @codeReviewId AND crc.is_resolved = false
            ORDER BY crc.line_number ASC, crc.created_at ASC";

        var comments = await _sqlExecutor.QueryAsync<CodeReviewComment, dynamic, CodeReviewComment>(
            sql,
            (comment, user) =>
            {
                comment.User = new Core.Models.Auth.User
                {
                    Id = user.id,
                    Username = user.username,
                    Email = user.email,
                    FirstName = user.first_name,
                    LastName = user.last_name,
                    ProfilePictureUrl = user.profile_picture_url
                };
                return comment;
            },
            new { codeReviewId },
            splitOn: "id");

        return comments;
    }

    public async Task<int> GetTotalCountByCodeReviewIdAsync(Guid codeReviewId)
    {
        var sql = "SELECT COUNT(*) FROM code_review_comments WHERE code_review_id = @codeReviewId";
        return await _sqlExecutor.QueryFirstAsync<int>(sql, new { codeReviewId });
    }
}