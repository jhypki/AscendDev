using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Courses;
using Microsoft.Extensions.Logging;

namespace AscendDev.Data.Repositories;

public class UserProgressRepository(
    ILogger<UserProgressRepository> logger,
    ISqlExecutor sql) : IUserProgressRepository
{
    public async Task<List<UserProgress>> GetByUserId(Guid userId)
    {
        const string query = """
                                 SELECT p.id, p.user_id, p.lesson_id, p.completed_at, p.code_solution,
                                        l.title as lesson_title, l.course_id
                                 FROM user_progress p
                                 JOIN lessons l ON p.lesson_id = l.id
                                 WHERE p.user_id = @UserId
                                 ORDER BY p.completed_at DESC
                             """;

        try
        {
            var result = await sql.QueryAsync<UserProgress>(query, new { UserId = userId });
            return result?.ToList() ?? new List<UserProgress>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving progress for user {UserId}", userId);
            throw new Exception($"Error retrieving progress for user {userId}", ex);
        }
    }

    public async Task<List<UserProgress>> GetByLessonId(string lessonId)
    {
        const string query = """
                                 SELECT p.id, p.user_id, p.lesson_id, p.completed_at, p.code_solution,
                                        u.username, u.email
                                 FROM user_progress p
                                 JOIN users u ON p.user_id = u.id
                                 WHERE p.lesson_id = @LessonId
                                 ORDER BY p.completed_at DESC
                             """;

        try
        {
            var result = await sql.QueryAsync<UserProgress>(query, new { LessonId = lessonId });
            return result?.ToList() ?? new List<UserProgress>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving progress for lesson {LessonId}", lessonId);
            throw new Exception($"Error retrieving progress for lesson {lessonId}", ex);
        }
    }

    public async Task<UserProgress?> GetByUserAndLessonId(Guid userId, string lessonId)
    {
        const string query = """
                                 SELECT id, user_id, lesson_id, completed_at, code_solution
                                 FROM user_progress
                                 WHERE user_id = @UserId AND lesson_id = @LessonId
                             """;

        try
        {
            return await sql.QueryFirstOrDefaultAsync<UserProgress>(query, new { UserId = userId, LessonId = lessonId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving progress for user {UserId} and lesson {LessonId}", userId, lessonId);
            throw new Exception($"Error retrieving progress for user {userId} and lesson {lessonId}", ex);
        }
    }

    public async Task<UserProgress> Create(UserProgress progress)
    {
        const string query = """
                                 INSERT INTO user_progress (user_id, lesson_id, completed_at, code_solution)
                                 VALUES (@UserId, @LessonId, @CompletedAt, @CodeSolution)
                                 RETURNING id, user_id, lesson_id, completed_at, code_solution
                             """;

        try
        {
            var result = await sql.QueryFirstOrDefaultAsync<UserProgress>(query, new
            {
                UserId = progress.UserId,
                LessonId = progress.LessonId,
                CompletedAt = progress.CompletedAt,
                CodeSolution = progress.CodeSolution
            });

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating progress for user {UserId} and lesson {LessonId}",
                progress.UserId, progress.LessonId);
            throw new Exception($"Error creating progress for user {progress.UserId} and lesson {progress.LessonId}", ex);
        }
    }

    public async Task<bool> Update(UserProgress progress)
    {
        const string query = """
                                 UPDATE user_progress
                                 SET completed_at = @CompletedAt,
                                     code_solution = @CodeSolution
                                 WHERE id = @Id
                             """;

        try
        {
            var rowsAffected = await sql.ExecuteAsync(query, new
            {
                Id = progress.Id,
                CompletedAt = progress.CompletedAt,
                CodeSolution = progress.CodeSolution
            });

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating progress with ID {Id}", progress.Id);
            throw new Exception($"Error updating progress with ID {progress.Id}", ex);
        }
    }

    public async Task<bool> Delete(int id)
    {
        const string query = """
                                 DELETE FROM user_progress
                                 WHERE id = @Id
                             """;

        try
        {
            var rowsAffected = await sql.ExecuteAsync(query, new { Id = id });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting progress with ID {Id}", id);
            throw new Exception($"Error deleting progress with ID {id}", ex);
        }
    }

    public async Task<bool> HasUserCompletedLesson(Guid userId, string lessonId)
    {
        const string query = """
                                 SELECT COUNT(1)
                                 FROM user_progress
                                 WHERE user_id = @UserId AND lesson_id = @LessonId
                             """;

        try
        {
            var count = await sql.QueryFirstOrDefaultAsync<int>(query, new { UserId = userId, LessonId = lessonId });
            return count > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking if user {UserId} completed lesson {LessonId}", userId, lessonId);
            throw new Exception($"Error checking if user {userId} completed lesson {lessonId}", ex);
        }
    }

    public async Task<int> GetCompletedLessonCountForCourse(Guid userId, string courseId)
    {
        const string query = """
                                 SELECT COUNT(1)
                                 FROM user_progress p
                                 JOIN lessons l ON p.lesson_id = l.id
                                 WHERE p.user_id = @UserId AND l.course_id = @CourseId
                             """;

        try
        {
            return await sql.QueryFirstOrDefaultAsync<int>(query, new { UserId = userId, CourseId = courseId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting completed lesson count for user {UserId} and course {CourseId}",
                userId, courseId);
            throw new Exception($"Error getting completed lesson count for user {userId} and course {courseId}", ex);
        }
    }
}