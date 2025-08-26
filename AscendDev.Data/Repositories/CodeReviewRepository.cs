using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Social;
using Dapper;

namespace AscendDev.Data.Repositories;

public class CodeReviewRepository : ICodeReviewRepository
{
    private readonly ISqlExecutor _sqlExecutor;

    public CodeReviewRepository(ISqlExecutor sqlExecutor)
    {
        _sqlExecutor = sqlExecutor;
    }

    public async Task<CodeReview?> GetByIdAsync(Guid id)
    {
        var sql = "SELECT * FROM code_reviews WHERE id = @id";
        var codeReview = await _sqlExecutor.QueryFirstOrDefaultAsync<CodeReview>(sql, new { id });

        if (codeReview != null)
        {
            await LoadUsersAsync(new[] { codeReview });
        }

        return codeReview;
    }

    public async Task<IEnumerable<CodeReview>> GetByLessonIdAsync(string lessonId, int page = 1, int pageSize = 20)
    {
        var offset = (page - 1) * pageSize;
        var sql = @"
            SELECT * FROM code_reviews 
            WHERE lesson_id = @lessonId
            ORDER BY created_at DESC
            LIMIT @pageSize OFFSET @offset";

        var codeReviews = await _sqlExecutor.QueryAsync<CodeReview>(sql, new { lessonId, pageSize, offset });
        await LoadUsersAsync(codeReviews);
        return codeReviews;
    }

    public async Task<IEnumerable<CodeReview>> GetByReviewerIdAsync(Guid reviewerId, int page = 1, int pageSize = 20)
    {
        var offset = (page - 1) * pageSize;
        var sql = @"
            SELECT * FROM code_reviews 
            WHERE reviewer_id = @reviewerId
            ORDER BY created_at DESC
            LIMIT @pageSize OFFSET @offset";

        var codeReviews = await _sqlExecutor.QueryAsync<CodeReview>(sql, new { reviewerId, pageSize, offset });
        await LoadUsersAsync(codeReviews);
        return codeReviews;
    }

    public async Task<IEnumerable<CodeReview>> GetByRevieweeIdAsync(Guid revieweeId, int page = 1, int pageSize = 20)
    {
        var offset = (page - 1) * pageSize;
        var sql = @"
            SELECT * FROM code_reviews 
            WHERE reviewee_id = @revieweeId
            ORDER BY created_at DESC
            LIMIT @pageSize OFFSET @offset";

        var codeReviews = await _sqlExecutor.QueryAsync<CodeReview>(sql, new { revieweeId, pageSize, offset });
        await LoadUsersAsync(codeReviews);
        return codeReviews;
    }

    public async Task<IEnumerable<CodeReview>> GetByStatusAsync(CodeReviewStatus status, int page = 1, int pageSize = 20)
    {
        var offset = (page - 1) * pageSize;
        var sql = @"
            SELECT * FROM code_reviews 
            WHERE status = @status
            ORDER BY created_at DESC
            LIMIT @pageSize OFFSET @offset";

        var codeReviews = await _sqlExecutor.QueryAsync<CodeReview>(sql, new { status, pageSize, offset });
        await LoadUsersAsync(codeReviews);
        return codeReviews;
    }

    public async Task<CodeReview> CreateAsync(CodeReview codeReview)
    {
        var sql = @"
            INSERT INTO code_reviews (id, lesson_id, reviewer_id, reviewee_id, code_solution, status, created_at)
            VALUES (@Id, @LessonId, @ReviewerId, @RevieweeId, @CodeSolution, @Status, @CreatedAt)
            RETURNING *";

        var result = await _sqlExecutor.QueryFirstAsync<CodeReview>(sql, codeReview);
        return result;
    }

    public async Task<CodeReview> UpdateAsync(CodeReview codeReview)
    {
        var sql = @"
            UPDATE code_reviews 
            SET status = @Status, code_solution = @CodeSolution, updated_at = @UpdatedAt, completed_at = @CompletedAt
            WHERE id = @Id
            RETURNING *";

        var result = await _sqlExecutor.QueryFirstAsync<CodeReview>(sql, codeReview);
        return result;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var sql = "DELETE FROM code_reviews WHERE id = @id";
        var rowsAffected = await _sqlExecutor.ExecuteAsync(sql, new { id });
        return rowsAffected > 0;
    }

    public async Task<int> GetTotalCountByLessonIdAsync(string lessonId)
    {
        var sql = "SELECT COUNT(*) FROM code_reviews WHERE lesson_id = @lessonId";
        return await _sqlExecutor.QueryFirstAsync<int>(sql, new { lessonId });
    }

    public async Task<int> GetTotalCountByReviewerIdAsync(Guid reviewerId)
    {
        var sql = "SELECT COUNT(*) FROM code_reviews WHERE reviewer_id = @reviewerId";
        return await _sqlExecutor.QueryFirstAsync<int>(sql, new { reviewerId });
    }

    public async Task<int> GetTotalCountByRevieweeIdAsync(Guid revieweeId)
    {
        var sql = "SELECT COUNT(*) FROM code_reviews WHERE reviewee_id = @revieweeId";
        return await _sqlExecutor.QueryFirstAsync<int>(sql, new { revieweeId });
    }

    public async Task<IEnumerable<CodeReview>> GetPendingReviewsAsync(int page = 1, int pageSize = 20)
    {
        var offset = (page - 1) * pageSize;
        var sql = @"
            SELECT * FROM code_reviews 
            WHERE status = 'Pending'
            ORDER BY created_at ASC
            LIMIT @pageSize OFFSET @offset";

        var codeReviews = await _sqlExecutor.QueryAsync<CodeReview>(sql, new { pageSize, offset });
        await LoadUsersAsync(codeReviews);
        return codeReviews;
    }

    private async Task LoadUsersAsync(IEnumerable<CodeReview> codeReviews)
    {
        var userIds = codeReviews.SelectMany(cr => new[] { cr.ReviewerId, cr.RevieweeId }).Distinct().ToList();

        if (!userIds.Any()) return;

        var userSql = @"
            SELECT id, username, email, first_name, last_name, profile_picture_url 
            FROM users 
            WHERE id = ANY(@userIds)";

        var users = await _sqlExecutor.QueryAsync<Core.Models.Auth.User>(userSql, new { userIds });
        var userDict = users.ToDictionary(u => u.Id);

        foreach (var codeReview in codeReviews)
        {
            if (userDict.TryGetValue(codeReview.ReviewerId, out var reviewer))
                codeReview.Reviewer = reviewer;

            if (userDict.TryGetValue(codeReview.RevieweeId, out var reviewee))
                codeReview.Reviewee = reviewee;
        }
    }
}