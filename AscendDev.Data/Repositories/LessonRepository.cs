using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Courses;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace AscendDev.Data.Repositories;

public class LessonRepository(
    ILogger<ILessonRepository> logger,
    ISqlExecutor sql) : ILessonRepository
{
    public async Task<List<Lesson>?> GetByCourseId(string courseId)
    {
        const string query = """
                                 SELECT id, course_id, title, slug, content, template, created_at, updated_at, language, "order",
                                        test_config, additional_resources, tags, status
                                 FROM lessons
                                 WHERE course_id = @CourseId
                             """;

        try
        {
            return await sql.QueryAsync<Lesson>(query, new { CourseId = courseId }) as List<Lesson>;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving lessons for course {CourseId}", courseId);
            throw new Exception($"Error retrieving lessons for course {courseId}", ex);
        }
    }

    public async Task<Lesson?> GetById(string id)
    {
        const string query = """
                                 SELECT id, course_id, title, slug, content, template, created_at, updated_at, language, "order",
                                        test_config, additional_resources, tags, status
                                 FROM lessons
                                 WHERE id = @Id
                             """;

        try
        {
            return await sql.QueryFirstOrDefaultAsync<Lesson>(query, new { Id = id });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving lesson by ID {Id}", id);
            throw new Exception($"Error retrieving lesson by ID {id}", ex);
        }
    }

    public async Task<Lesson?> GetBySlug(string slug)
    {
        const string query = """
                                 SELECT id, course_id, title, slug, content, template, created_at, updated_at, language, "order",
                                        test_config, additional_resources, tags, status
                                 FROM lessons
                                 WHERE slug = @Slug
                             """;

        try
        {
            return await sql.QueryFirstOrDefaultAsync<Lesson>(query, new { Slug = slug });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving lesson by slug {Slug}", slug);
            throw new Exception($"Error retrieving lesson by slug {slug}", ex);
        }
    }

    public async Task<List<Lesson>?> GetByStatus(string status)
    {
        const string query = """
                                 SELECT id, course_id, title, slug, content, template, created_at, updated_at, language, "order",
                                        test_config, additional_resources, tags, status
                                 FROM lessons
                                 WHERE status = @Status
                             """;

        try
        {
            return await sql.QueryAsync<Lesson>(query, new { Status = status }) as List<Lesson>;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving lessons by status {Status}", status);
            throw new Exception($"Error retrieving lessons by status {status}", ex);
        }
    }

    public async Task<List<Lesson>?> GetByCourseIdOrdered(string courseId)
    {
        const string query = """
                                 SELECT id, course_id, title, slug, content, template, created_at, updated_at, language, "order",
                                        test_config, additional_resources, tags, status
                                 FROM lessons
                                 WHERE course_id = @CourseId
                                 ORDER BY "order" ASC
                             """;

        try
        {
            return await sql.QueryAsync<Lesson>(query, new { CourseId = courseId }) as List<Lesson>;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving ordered lessons for course {CourseId}", courseId);
            throw new Exception($"Error retrieving ordered lessons for course {courseId}", ex);
        }
    }

    public async Task<Lesson?> GetNextLesson(string courseId, int currentOrder)
    {
        const string query = """
                                 SELECT id, course_id, title, slug, content, template, created_at, updated_at, language, "order",
                                        test_config, additional_resources, tags, status
                                 FROM lessons
                                 WHERE course_id = @CourseId AND "order" > @CurrentOrder
                                 ORDER BY "order" ASC
                                 LIMIT 1
                             """;

        try
        {
            return await sql.QueryFirstOrDefaultAsync<Lesson>(query, new { CourseId = courseId, CurrentOrder = currentOrder });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving next lesson for course {CourseId} after order {CurrentOrder}", courseId, currentOrder);
            throw new Exception($"Error retrieving next lesson for course {courseId} after order {currentOrder}", ex);
        }
    }

    public async Task<Lesson?> GetPreviousLesson(string courseId, int currentOrder)
    {
        const string query = """
                                 SELECT id, course_id, title, slug, content, template, created_at, updated_at, language, "order",
                                        test_config, additional_resources, tags, status
                                 FROM lessons
                                 WHERE course_id = @CourseId AND "order" < @CurrentOrder
                                 ORDER BY "order" DESC
                                 LIMIT 1
                             """;

        try
        {
            return await sql.QueryFirstOrDefaultAsync<Lesson>(query, new { CourseId = courseId, CurrentOrder = currentOrder });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving previous lesson for course {CourseId} before order {CurrentOrder}", courseId, currentOrder);
            throw new Exception($"Error retrieving previous lesson for course {courseId} before order {currentOrder}", ex);
        }
    }

    public async Task<Lesson> Create(Lesson lesson)
    {
        const string query = """
                                 INSERT INTO lessons (id, course_id, title, slug, content, template, created_at, updated_at,
                                                    language, "order", test_config, additional_resources, tags, status)
                                 VALUES (@Id, @CourseId, @Title, @Slug, @Content, @Template, @CreatedAt, @UpdatedAt,
                                        @Language, @Order, @TestConfig, @AdditionalResources, @Tags, @Status)
                                 RETURNING id, course_id, title, slug, content, template, created_at, updated_at, language, "order",
                                          test_config, additional_resources, tags, status
                             """;

        try
        {
            lesson.CreatedAt = DateTime.UtcNow;
            lesson.UpdatedAt = DateTime.UtcNow;

            return await sql.QueryFirstAsync<Lesson>(query, lesson);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating lesson {Title}", lesson.Title);
            throw new Exception($"Error creating lesson {lesson.Title}", ex);
        }
    }

    public async Task<Lesson> Update(Lesson lesson)
    {
        const string query = """
                                 UPDATE lessons
                                 SET title = @Title, slug = @Slug, content = @Content, template = @Template,
                                     updated_at = @UpdatedAt, language = @Language, "order" = @Order,
                                     test_config = @TestConfig, additional_resources = @AdditionalResources,
                                     tags = @Tags, status = @Status
                                 WHERE id = @Id
                                 RETURNING id, course_id, title, slug, content, template, created_at, updated_at, language, "order",
                                          test_config, additional_resources, tags, status
                             """;

        try
        {
            lesson.UpdatedAt = DateTime.UtcNow;
            return await sql.QueryFirstAsync<Lesson>(query, lesson);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating lesson {Id}", lesson.Id);
            throw new Exception($"Error updating lesson {lesson.Id}", ex);
        }
    }

    public async Task<bool> Delete(string lessonId)
    {
        const string query = "DELETE FROM lessons WHERE id = @LessonId";

        try
        {
            var rowsAffected = await sql.ExecuteAsync(query, new { LessonId = lessonId });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting lesson {LessonId}", lessonId);
            throw new Exception($"Error deleting lesson {lessonId}", ex);
        }
    }

    public async Task<bool> UpdateStatus(string lessonId, string status)
    {
        const string query = """
                                 UPDATE lessons
                                 SET status = @Status, updated_at = @UpdatedAt
                                 WHERE id = @LessonId
                             """;

        try
        {
            var rowsAffected = await sql.ExecuteAsync(query, new
            {
                LessonId = lessonId,
                Status = status,
                UpdatedAt = DateTime.UtcNow
            });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating lesson status {LessonId}", lessonId);
            throw new Exception($"Error updating lesson status {lessonId}", ex);
        }
    }

    public async Task<bool> UpdateOrder(string lessonId, int newOrder)
    {
        const string query = """
                                 UPDATE lessons
                                 SET "order" = @NewOrder, updated_at = @UpdatedAt
                                 WHERE id = @LessonId
                             """;

        try
        {
            var rowsAffected = await sql.ExecuteAsync(query, new
            {
                LessonId = lessonId,
                NewOrder = newOrder,
                UpdatedAt = DateTime.UtcNow
            });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating lesson order {LessonId}", lessonId);
            throw new Exception($"Error updating lesson order {lessonId}", ex);
        }
    }

    public async Task<bool> ReorderLessons(string courseId, List<string> lessonIds)
    {
        const string query = """
                                 UPDATE lessons
                                 SET "order" = @NewOrder, updated_at = @UpdatedAt
                                 WHERE id = @LessonId AND course_id = @CourseId
                             """;

        try
        {
            var updateTasks = lessonIds.Select((lessonId, index) =>
                sql.ExecuteAsync(query, new
                {
                    LessonId = lessonId,
                    CourseId = courseId,
                    NewOrder = index + 1,
                    UpdatedAt = DateTime.UtcNow
                })
            );

            var results = await Task.WhenAll(updateTasks);
            return results.All(result => result > 0);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reordering lessons for course {CourseId}", courseId);
            throw new Exception($"Error reordering lessons for course {courseId}", ex);
        }
    }

    public async Task<bool> ValidateLesson(Lesson lesson)
    {
        var errors = await GetValidationErrors(lesson);
        return errors.Count == 0;
    }

    public async Task<List<string>> GetValidationErrors(Lesson lesson)
    {
        var errors = new List<string>();

        // Basic validation
        if (string.IsNullOrWhiteSpace(lesson.Title))
            errors.Add("Title is required");

        if (string.IsNullOrWhiteSpace(lesson.Slug))
            errors.Add("Slug is required");

        if (string.IsNullOrWhiteSpace(lesson.Content))
            errors.Add("Content is required");

        if (string.IsNullOrWhiteSpace(lesson.Template))
            errors.Add("Template is required");

        if (string.IsNullOrWhiteSpace(lesson.Language))
            errors.Add("Language is required");

        // Check for duplicate slug in the same course
        if (!string.IsNullOrWhiteSpace(lesson.Slug) && !string.IsNullOrWhiteSpace(lesson.CourseId))
        {
            const string duplicateSlugQuery = """
                                                 SELECT COUNT(*)
                                                 FROM lessons
                                                 WHERE slug = @Slug AND course_id = @CourseId AND id != @Id
                                             """;

            try
            {
                var duplicateCount = await sql.QueryFirstAsync<int>(duplicateSlugQuery, new
                {
                    Slug = lesson.Slug,
                    CourseId = lesson.CourseId,
                    Id = lesson.Id ?? ""
                });

                if (duplicateCount > 0)
                    errors.Add("Slug must be unique within the course");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking for duplicate slug");
                errors.Add("Error validating slug uniqueness");
            }
        }

        return errors;
    }

    // Extended methods for analytics
    public async Task<int> CountAsync()
    {
        const string query = "SELECT COUNT(*) FROM lessons";
        try
        {
            return await sql.QueryFirstAsync<int>(query);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error counting lessons");
            throw new Exception("Error counting lessons", ex);
        }
    }

    public async Task<int> CountAsync(Expression<Func<Lesson, bool>> predicate)
    {
        // Simplified implementation for common cases
        if (predicate.Body is BinaryExpression binaryExpr)
        {
            // Handle date comparison for CreatedAt
            if (binaryExpr.Left is MemberExpression dateMember && dateMember.Member.Name == "CreatedAt" &&
                binaryExpr.NodeType == ExpressionType.GreaterThanOrEqual &&
                binaryExpr.Right is ConstantExpression dateConstant && dateConstant.Value is DateTime date)
            {
                const string query = "SELECT COUNT(*) FROM lessons WHERE created_at >= @Date AND created_at < @EndDate";
                try
                {
                    return await sql.QueryFirstAsync<int>(query, new { Date = date, EndDate = date.AddDays(1) });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error counting lessons by date");
                    throw new Exception("Error counting lessons by date", ex);
                }
            }
        }

        // Fallback - get all lessons and filter in memory
        logger.LogWarning("Using inefficient in-memory filtering for lesson count");
        var allLessons = await GetAllAsync();
        return allLessons.Where(predicate.Compile()).Count();
    }

    public async Task<List<Lesson>> GetAllAsync()
    {
        const string query = """
                                     SELECT id, course_id, title, slug, content, template, created_at, updated_at, language, "order",
                                            test_config, additional_resources, tags, status
                                     FROM lessons
                             """;
        try
        {
            var result = await sql.QueryAsync<Lesson>(query);
            return result.ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all lessons");
            throw new Exception("Error getting all lessons", ex);
        }
    }

    public IQueryable<Lesson> GetQueryable()
    {
        // Simplified implementation - return empty queryable
        // In a real EF Core scenario, this would return the DbSet
        return new List<Lesson>().AsQueryable();
    }
}