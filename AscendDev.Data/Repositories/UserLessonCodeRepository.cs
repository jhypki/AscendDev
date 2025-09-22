using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Courses;
using Microsoft.Extensions.Logging;

namespace AscendDev.Data.Repositories;

public class UserLessonCodeRepository(ISqlExecutor sqlExecutor, ILogger<UserLessonCodeRepository> logger) : IUserLessonCodeRepository
{
    public async Task<UserLessonCode?> GetUserCodeAsync(Guid userId, string lessonId)
    {
        const string sql = @"
            SELECT id, user_id as UserId, lesson_id as LessonId, code, created_at as CreatedAt, updated_at as UpdatedAt
            FROM user_lesson_code
            WHERE user_id = @UserId AND lesson_id = @LessonId";

        try
        {
            var result = await sqlExecutor.QueryFirstOrDefaultAsync<UserLessonCode>(sql, new { UserId = userId, LessonId = lessonId });
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user code for user {UserId} and lesson {LessonId}", userId, lessonId);
            throw;
        }
    }

    public async Task<UserLessonCode> SaveUserCodeAsync(Guid userId, string lessonId, string code)
    {
        const string upsertSql = @"
            INSERT INTO user_lesson_code (user_id, lesson_id, code, created_at, updated_at)
            VALUES (@UserId, @LessonId, @Code, @Now, @Now)
            ON CONFLICT (user_id, lesson_id)
            DO UPDATE SET
                code = EXCLUDED.code,
                updated_at = EXCLUDED.updated_at
            RETURNING id, user_id as UserId, lesson_id as LessonId, code, created_at as CreatedAt, updated_at as UpdatedAt";

        try
        {
            var now = DateTime.UtcNow;
            var result = await sqlExecutor.QueryFirstAsync<UserLessonCode>(upsertSql, new
            {
                UserId = userId,
                LessonId = lessonId,
                Code = code,
                Now = now
            });

            logger.LogInformation("Saved user code for user {UserId} and lesson {LessonId}", userId, lessonId);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving user code for user {UserId} and lesson {LessonId}", userId, lessonId);
            throw;
        }
    }

    public async Task<bool> DeleteUserCodeAsync(Guid userId, string lessonId)
    {
        const string sql = @"
            DELETE FROM user_lesson_code
            WHERE user_id = @UserId AND lesson_id = @LessonId";

        try
        {
            var rowsAffected = await sqlExecutor.ExecuteAsync(sql, new { UserId = userId, LessonId = lessonId });

            logger.LogInformation("Deleted user code for user {UserId} and lesson {LessonId}", userId, lessonId);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting user code for user {UserId} and lesson {LessonId}", userId, lessonId);
            throw;
        }
    }
}