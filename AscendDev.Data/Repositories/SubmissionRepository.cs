using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Courses;
using Dapper;

namespace AscendDev.Data.Repositories;

public class SubmissionRepository : ISubmissionRepository
{
    private readonly ISqlExecutor _sqlExecutor;

    public SubmissionRepository(ISqlExecutor sqlExecutor)
    {
        _sqlExecutor = sqlExecutor;
    }

    public async Task<Submission?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT s.*, u.username, u.first_name, u.last_name, u.profile_picture_url,
                   l.title as lesson_title, l.slug as lesson_slug
            FROM submissions s
            INNER JOIN users u ON s.user_id = u.id
            INNER JOIN lessons l ON s.lesson_id = l.id
            WHERE s.id = @Id";

        var result = await _sqlExecutor.QueryAsync<dynamic>(sql, new { Id = id });
        var row = result.FirstOrDefault();

        if (row == null) return null;

        return new Submission
        {
            Id = row.id,
            UserId = row.user_id,
            LessonId = row.lesson_id,
            Code = row.code,
            Passed = row.passed,
            SubmittedAt = row.submitted_at,
            TestResults = row.test_results,
            ExecutionTimeMs = row.execution_time_ms,
            ErrorMessage = row.error_message,
            User = new()
            {
                Id = row.user_id,
                Username = row.username,
                FirstName = row.first_name,
                LastName = row.last_name,
                ProfilePictureUrl = row.profile_picture_url
            },
            Lesson = new()
            {
                Id = row.lesson_id,
                Title = row.lesson_title,
                Slug = row.lesson_slug
            }
        };
    }

    public async Task<IEnumerable<Submission>> GetByUserIdAsync(Guid userId)
    {
        const string sql = @"
            SELECT s.*, l.title as lesson_title, l.slug as lesson_slug
            FROM submissions s
            INNER JOIN lessons l ON s.lesson_id = l.id
            WHERE s.user_id = @UserId
            ORDER BY s.submitted_at DESC";

        var results = await _sqlExecutor.QueryAsync<dynamic>(sql, new { UserId = userId });

        return results.Select(row => new Submission
        {
            Id = row.id,
            UserId = row.user_id,
            LessonId = row.lesson_id,
            Code = row.code,
            Passed = row.passed,
            SubmittedAt = row.submitted_at,
            TestResults = row.test_results,
            ExecutionTimeMs = row.execution_time_ms,
            ErrorMessage = row.error_message,
            Lesson = new()
            {
                Id = row.lesson_id,
                Title = row.lesson_title,
                Slug = row.lesson_slug
            }
        });
    }

    public async Task<IEnumerable<Submission>> GetByLessonIdAsync(string lessonId)
    {
        const string sql = @"
            SELECT s.*, u.username, u.first_name, u.last_name, u.profile_picture_url
            FROM submissions s
            INNER JOIN users u ON s.user_id = u.id
            WHERE s.lesson_id = @LessonId
            ORDER BY s.submitted_at DESC";

        var results = await _sqlExecutor.QueryAsync<dynamic>(sql, new { LessonId = lessonId });

        return results.Select(row => new Submission
        {
            Id = row.id,
            UserId = row.user_id,
            LessonId = row.lesson_id,
            Code = row.code,
            Passed = row.passed,
            SubmittedAt = row.submitted_at,
            TestResults = row.test_results,
            ExecutionTimeMs = row.execution_time_ms,
            ErrorMessage = row.error_message,
            User = new()
            {
                Id = row.user_id,
                Username = row.username,
                FirstName = row.first_name,
                LastName = row.last_name,
                ProfilePictureUrl = row.profile_picture_url
            }
        });
    }

    public async Task<Submission?> GetLatestByUserAndLessonAsync(Guid userId, string lessonId)
    {
        const string sql = @"
            SELECT * FROM submissions 
            WHERE user_id = @UserId AND lesson_id = @LessonId
            ORDER BY submitted_at DESC
            LIMIT 1";

        return await _sqlExecutor.QueryFirstOrDefaultAsync<Submission>(sql, new { UserId = userId, LessonId = lessonId });
    }

    public async Task<IEnumerable<Submission>> GetPublicPassedSubmissionsAsync(string lessonId, int limit = 50)
    {
        const string sql = @"
            SELECT s.*, u.username, u.first_name, u.last_name, u.profile_picture_url,
                   l.title as lesson_title, l.slug as lesson_slug
            FROM submissions s
            INNER JOIN users u ON s.user_id = u.id
            INNER JOIN lessons l ON s.lesson_id = l.id
            INNER JOIN user_settings us ON s.user_id = us.user_id
            WHERE s.lesson_id = @LessonId 
              AND s.passed = true 
              AND us.public_submissions = true
            ORDER BY s.submitted_at DESC
            LIMIT @Limit";

        var results = await _sqlExecutor.QueryAsync<dynamic>(sql, new { LessonId = lessonId, Limit = limit });

        return results.Select(row => new Submission
        {
            Id = row.id,
            UserId = row.user_id,
            LessonId = row.lesson_id,
            Code = row.code,
            Passed = row.passed,
            SubmittedAt = row.submitted_at,
            TestResults = row.test_results,
            ExecutionTimeMs = row.execution_time_ms,
            ErrorMessage = row.error_message,
            User = new()
            {
                Id = row.user_id,
                Username = row.username,
                FirstName = row.first_name,
                LastName = row.last_name,
                ProfilePictureUrl = row.profile_picture_url
            },
            Lesson = new()
            {
                Id = row.lesson_id,
                Title = row.lesson_title,
                Slug = row.lesson_slug
            }
        });
    }

    public async Task<IEnumerable<Submission>> GetUserPublicPassedSubmissionsAsync(Guid userId, int limit = 50)
    {
        const string sql = @"
            SELECT s.*, u.username, u.first_name, u.last_name, u.profile_picture_url,
                   l.title as lesson_title, l.slug as lesson_slug
            FROM submissions s
            INNER JOIN users u ON s.user_id = u.id
            INNER JOIN lessons l ON s.lesson_id = l.id
            INNER JOIN user_settings us ON s.user_id = us.user_id
            WHERE s.user_id = @UserId 
              AND s.passed = true 
              AND us.public_submissions = true
            ORDER BY s.submitted_at DESC
            LIMIT @Limit";

        var results = await _sqlExecutor.QueryAsync<dynamic>(sql, new { UserId = userId, Limit = limit });

        return results.Select(row => new Submission
        {
            Id = row.id,
            UserId = row.user_id,
            LessonId = row.lesson_id,
            Code = row.code,
            Passed = row.passed,
            SubmittedAt = row.submitted_at,
            TestResults = row.test_results,
            ExecutionTimeMs = row.execution_time_ms,
            ErrorMessage = row.error_message,
            User = new()
            {
                Id = row.user_id,
                Username = row.username,
                FirstName = row.first_name,
                LastName = row.last_name,
                ProfilePictureUrl = row.profile_picture_url
            },
            Lesson = new()
            {
                Id = row.lesson_id,
                Title = row.lesson_title,
                Slug = row.lesson_slug
            }
        });
    }

    public async Task<int> CreateAsync(Submission submission)
    {
        const string sql = @"
            INSERT INTO submissions (user_id, lesson_id, code, passed, submitted_at, test_results, execution_time_ms, error_message)
            VALUES (@UserId, @LessonId, @Code, @Passed, @SubmittedAt, @TestResults, @ExecutionTimeMs, @ErrorMessage)
            RETURNING id";

        return await _sqlExecutor.QuerySingleAsync<int>(sql, submission);
    }

    public async Task UpdateAsync(Submission submission)
    {
        const string sql = @"
            UPDATE submissions 
            SET code = @Code, passed = @Passed, submitted_at = @SubmittedAt, 
                test_results = @TestResults, execution_time_ms = @ExecutionTimeMs, error_message = @ErrorMessage
            WHERE id = @Id";

        await _sqlExecutor.ExecuteAsync(sql, submission);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM submissions WHERE id = @Id";
        await _sqlExecutor.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<bool> ExistsAsync(int id)
    {
        const string sql = "SELECT COUNT(1) FROM submissions WHERE id = @Id";
        var count = await _sqlExecutor.QuerySingleAsync<int>(sql, new { Id = id });
        return count > 0;
    }

    public async Task<int> GetSubmissionCountByUserAsync(Guid userId)
    {
        const string sql = "SELECT COUNT(*) FROM submissions WHERE user_id = @UserId";
        return await _sqlExecutor.QuerySingleAsync<int>(sql, new { UserId = userId });
    }

    public async Task<int> GetPassedSubmissionCountByUserAsync(Guid userId)
    {
        const string sql = "SELECT COUNT(*) FROM submissions WHERE user_id = @UserId AND passed = true";
        return await _sqlExecutor.QuerySingleAsync<int>(sql, new { UserId = userId });
    }

    public async Task<IEnumerable<Submission>> GetSubmissionsForReviewAsync(string lessonId, int limit = 50)
    {
        const string sql = @"
            SELECT s.*, u.username, u.first_name, u.last_name, u.profile_picture_url,
                   l.title as lesson_title, l.slug as lesson_slug
            FROM submissions s
            INNER JOIN users u ON s.user_id = u.id
            INNER JOIN lessons l ON s.lesson_id = l.id
            INNER JOIN user_settings us ON s.user_id = us.user_id
            WHERE s.lesson_id = @LessonId
              AND s.passed = true
              AND us.public_submissions = true
              AND NOT EXISTS (
                  SELECT 1 FROM code_reviews cr
                  WHERE cr.submission_id = s.id
                  AND cr.status IN ('pending', 'in_review')
              )
            ORDER BY s.submitted_at DESC
            LIMIT @Limit";

        var results = await _sqlExecutor.QueryAsync<dynamic>(sql, new { LessonId = lessonId, Limit = limit });

        return results.Select(row => new Submission
        {
            Id = row.id,
            UserId = row.user_id,
            LessonId = row.lesson_id,
            Code = row.code,
            Passed = row.passed,
            SubmittedAt = row.submitted_at,
            TestResults = row.test_results,
            ExecutionTimeMs = row.execution_time_ms,
            ErrorMessage = row.error_message,
            User = new()
            {
                Id = row.user_id,
                Username = row.username,
                FirstName = row.first_name,
                LastName = row.last_name,
                ProfilePictureUrl = row.profile_picture_url
            },
            Lesson = new()
            {
                Id = row.lesson_id,
                Title = row.lesson_title,
                Slug = row.lesson_slug
            }
        });
    }

    public async Task<Submission?> GetSubmissionForReviewAsync(int submissionId)
    {
        const string sql = @"
            SELECT s.*, u.username, u.first_name, u.last_name, u.profile_picture_url,
                   l.title as lesson_title, l.slug as lesson_slug
            FROM submissions s
            INNER JOIN users u ON s.user_id = u.id
            INNER JOIN lessons l ON s.lesson_id = l.id
            INNER JOIN user_settings us ON s.user_id = us.user_id
            WHERE s.id = @SubmissionId
              AND s.passed = true
              AND us.public_submissions = true";

        var result = await _sqlExecutor.QueryAsync<dynamic>(sql, new { SubmissionId = submissionId });
        var row = result.FirstOrDefault();

        if (row == null) return null;

        return new Submission
        {
            Id = row.id,
            UserId = row.user_id,
            LessonId = row.lesson_id,
            Code = row.code,
            Passed = row.passed,
            SubmittedAt = row.submitted_at,
            TestResults = row.test_results,
            ExecutionTimeMs = row.execution_time_ms,
            ErrorMessage = row.error_message,
            User = new()
            {
                Id = row.user_id,
                Username = row.username,
                FirstName = row.first_name,
                LastName = row.last_name,
                ProfilePictureUrl = row.profile_picture_url
            },
            Lesson = new()
            {
                Id = row.lesson_id,
                Title = row.lesson_title,
                Slug = row.lesson_slug
            }
        };
    }
}