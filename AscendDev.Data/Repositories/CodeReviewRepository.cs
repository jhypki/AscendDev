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
            INSERT INTO code_reviews (id, lesson_id, reviewer_id, reviewee_id, submission_id, status, created_at)
            VALUES (@Id, @LessonId, @ReviewerId, @RevieweeId, @SubmissionId, @Status, @CreatedAt)
            RETURNING *";

        var result = await _sqlExecutor.QueryFirstAsync<CodeReview>(sql, codeReview);
        await LoadUsersAsync(new[] { result });
        return result;
    }

    public async Task<CodeReview> UpdateAsync(CodeReview codeReview)
    {
        var sql = @"
            UPDATE code_reviews
            SET status = @Status, updated_at = @UpdatedAt, completed_at = @CompletedAt
            WHERE id = @Id
            RETURNING *";

        var result = await _sqlExecutor.QueryFirstAsync<CodeReview>(sql, codeReview);
        await LoadUsersAsync(new[] { result });
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

    public async Task<CodeReview?> GetBySubmissionAndReviewerAsync(int submissionId, Guid reviewerId)
    {
        var sql = @"
            SELECT * FROM code_reviews
            WHERE submission_id = @submissionId AND reviewer_id = @reviewerId
            ORDER BY created_at DESC
            LIMIT 1";

        var codeReview = await _sqlExecutor.QueryFirstOrDefaultAsync<CodeReview>(sql, new { submissionId, reviewerId });

        if (codeReview != null)
        {
            await LoadUsersAsync(new[] { codeReview });
        }

        return codeReview;
    }

    public async Task<IEnumerable<CodeReview>> GetBySubmissionIdAsync(int submissionId)
    {
        var sql = @"
            SELECT * FROM code_reviews
            WHERE submission_id = @submissionId
            ORDER BY created_at DESC";

        var codeReviews = await _sqlExecutor.QueryAsync<CodeReview>(sql, new { submissionId });
        await LoadUsersAsync(codeReviews);
        return codeReviews;
    }

    private async Task LoadUsersAsync(IEnumerable<CodeReview> codeReviews)
    {
        var codeReviewsList = codeReviews.ToList();

        // Load users
        var userIds = codeReviewsList.SelectMany(cr => new[] { cr.ReviewerId, cr.RevieweeId }).Distinct().ToList();

        if (userIds.Any())
        {
            var userSql = @"
                SELECT id, username, email, first_name, last_name, profile_picture_url
                FROM users
                WHERE id = ANY(@userIds)";

            var users = await _sqlExecutor.QueryAsync<Core.Models.Auth.User>(userSql, new { userIds });
            var userDict = users.ToDictionary(u => u.Id);

            foreach (var codeReview in codeReviewsList)
            {
                if (userDict.TryGetValue(codeReview.ReviewerId, out var reviewer))
                    codeReview.Reviewer = reviewer;

                if (userDict.TryGetValue(codeReview.RevieweeId, out var reviewee))
                    codeReview.Reviewee = reviewee;
            }
        }

        // Load comments
        var codeReviewIds = codeReviewsList.Select(cr => cr.Id).ToList();

        if (codeReviewIds.Any())
        {
            var commentSql = @"
                SELECT crc.*, u.id, u.username, u.email, u.first_name, u.last_name, u.profile_picture_url
                FROM code_review_comments crc
                INNER JOIN users u ON crc.user_id = u.id
                WHERE crc.code_review_id = ANY(@codeReviewIds)
                ORDER BY crc.created_at ASC";

            var commentData = await _sqlExecutor.QueryAsync<CodeReviewComment, Core.Models.Auth.User, CodeReviewComment>(
                commentSql,
                (comment, user) =>
                {
                    comment.User = user;
                    return comment;
                },
                new { codeReviewIds },
                splitOn: "id");

            var commentsByReview = commentData.GroupBy(c => c.CodeReviewId).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var codeReview in codeReviewsList)
            {
                if (commentsByReview.TryGetValue(codeReview.Id, out var comments))
                {
                    codeReview.Comments = comments;
                }
            }
        }
    }
}