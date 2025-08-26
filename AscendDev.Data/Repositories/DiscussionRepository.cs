using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Social;
using Dapper;

namespace AscendDev.Data.Repositories;

public class DiscussionRepository : IDiscussionRepository
{
    private readonly ISqlExecutor _sqlExecutor;

    public DiscussionRepository(ISqlExecutor sqlExecutor)
    {
        _sqlExecutor = sqlExecutor;
    }

    public async Task<Discussion?> GetByIdAsync(Guid id)
    {
        var sql = @"
            SELECT d.*, u.id, u.username, u.email, u.first_name, u.last_name, u.profile_picture_url
            FROM discussions d
            INNER JOIN users u ON d.user_id = u.id
            WHERE d.id = @id";

        var discussions = await _sqlExecutor.QueryAsync<Discussion, dynamic, Discussion>(
            sql,
            (discussion, user) =>
            {
                discussion.User = new Core.Models.Auth.User
                {
                    Id = user.id,
                    Username = user.username,
                    Email = user.email,
                    FirstName = user.first_name,
                    LastName = user.last_name,
                    ProfilePictureUrl = user.profile_picture_url
                };
                return discussion;
            },
            new { id },
            splitOn: "id");

        return discussions.FirstOrDefault();
    }

    public async Task<IEnumerable<Discussion>> GetByLessonIdAsync(string lessonId, int page = 1, int pageSize = 20)
    {
        var offset = (page - 1) * pageSize;
        var sql = @"
            SELECT d.*, u.id, u.username, u.email, u.first_name, u.last_name, u.profile_picture_url
            FROM discussions d
            INNER JOIN users u ON d.user_id = u.id
            WHERE d.lesson_id = @lessonId
            ORDER BY d.is_pinned DESC, d.created_at DESC
            LIMIT @pageSize OFFSET @offset";

        var discussions = await _sqlExecutor.QueryAsync<Discussion, dynamic, Discussion>(
            sql,
            (discussion, user) =>
            {
                discussion.User = new Core.Models.Auth.User
                {
                    Id = user.id,
                    Username = user.username,
                    Email = user.email,
                    FirstName = user.first_name,
                    LastName = user.last_name,
                    ProfilePictureUrl = user.profile_picture_url
                };
                return discussion;
            },
            new { lessonId, pageSize, offset },
            splitOn: "id");

        return discussions;
    }

    public async Task<IEnumerable<Discussion>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var offset = (page - 1) * pageSize;
        var sql = @"
            SELECT d.*, u.id, u.username, u.email, u.first_name, u.last_name, u.profile_picture_url
            FROM discussions d
            INNER JOIN users u ON d.user_id = u.id
            WHERE d.user_id = @userId
            ORDER BY d.created_at DESC
            LIMIT @pageSize OFFSET @offset";

        var discussions = await _sqlExecutor.QueryAsync<Discussion, dynamic, Discussion>(
            sql,
            (discussion, user) =>
            {
                discussion.User = new Core.Models.Auth.User
                {
                    Id = user.id,
                    Username = user.username,
                    Email = user.email,
                    FirstName = user.first_name,
                    LastName = user.last_name,
                    ProfilePictureUrl = user.profile_picture_url
                };
                return discussion;
            },
            new { userId, pageSize, offset },
            splitOn: "id");

        return discussions;
    }

    public async Task<Discussion> CreateAsync(Discussion discussion)
    {
        var sql = @"
            INSERT INTO discussions (id, lesson_id, user_id, title, content, created_at, is_pinned, is_locked, view_count)
            VALUES (@Id, @LessonId, @UserId, @Title, @Content, @CreatedAt, @IsPinned, @IsLocked, @ViewCount)
            RETURNING *";

        var result = await _sqlExecutor.QueryFirstAsync<Discussion>(sql, discussion);
        return result;
    }

    public async Task<Discussion> UpdateAsync(Discussion discussion)
    {
        var sql = @"
            UPDATE discussions 
            SET title = @Title, content = @Content, updated_at = @UpdatedAt, 
                is_pinned = @IsPinned, is_locked = @IsLocked
            WHERE id = @Id
            RETURNING *";

        var result = await _sqlExecutor.QueryFirstAsync<Discussion>(sql, discussion);
        return result;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var sql = "DELETE FROM discussions WHERE id = @id";
        var rowsAffected = await _sqlExecutor.ExecuteAsync(sql, new { id });
        return rowsAffected > 0;
    }

    public async Task<bool> IncrementViewCountAsync(Guid id)
    {
        var sql = "UPDATE discussions SET view_count = view_count + 1 WHERE id = @id";
        var rowsAffected = await _sqlExecutor.ExecuteAsync(sql, new { id });
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<Discussion>> GetPinnedByLessonIdAsync(string lessonId)
    {
        var sql = @"
            SELECT d.*, u.id, u.username, u.email, u.first_name, u.last_name, u.profile_picture_url
            FROM discussions d
            INNER JOIN users u ON d.user_id = u.id
            WHERE d.lesson_id = @lessonId AND d.is_pinned = true
            ORDER BY d.created_at DESC";

        var discussions = await _sqlExecutor.QueryAsync<Discussion, dynamic, Discussion>(
            sql,
            (discussion, user) =>
            {
                discussion.User = new Core.Models.Auth.User
                {
                    Id = user.id,
                    Username = user.username,
                    Email = user.email,
                    FirstName = user.first_name,
                    LastName = user.last_name,
                    ProfilePictureUrl = user.profile_picture_url
                };
                return discussion;
            },
            new { lessonId },
            splitOn: "id");

        return discussions;
    }

    public async Task<int> GetTotalCountByLessonIdAsync(string lessonId)
    {
        var sql = "SELECT COUNT(*) FROM discussions WHERE lesson_id = @lessonId";
        return await _sqlExecutor.QueryFirstAsync<int>(sql, new { lessonId });
    }

    public async Task<IEnumerable<Discussion>> SearchAsync(string searchTerm, string? lessonId = null, int page = 1, int pageSize = 20)
    {
        var offset = (page - 1) * pageSize;
        var whereClause = lessonId != null ? "AND d.lesson_id = @lessonId" : "";

        var sql = $@"
            SELECT d.*, u.id, u.username, u.email, u.first_name, u.last_name, u.profile_picture_url
            FROM discussions d
            INNER JOIN users u ON d.user_id = u.id
            WHERE (d.title ILIKE @searchTerm OR d.content ILIKE @searchTerm) {whereClause}
            ORDER BY d.created_at DESC
            LIMIT @pageSize OFFSET @offset";

        var discussions = await _sqlExecutor.QueryAsync<Discussion, dynamic, Discussion>(
            sql,
            (discussion, user) =>
            {
                discussion.User = new Core.Models.Auth.User
                {
                    Id = user.id,
                    Username = user.username,
                    Email = user.email,
                    FirstName = user.first_name,
                    LastName = user.last_name,
                    ProfilePictureUrl = user.profile_picture_url
                };
                return discussion;
            },
            new { searchTerm = $"%{searchTerm}%", lessonId, pageSize, offset },
            splitOn: "id");

        return discussions;
    }
}