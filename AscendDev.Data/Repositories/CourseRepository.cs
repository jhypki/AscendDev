using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Courses;
using Microsoft.Extensions.Logging;

namespace AscendDev.Data.Repositories;

public class CourseRepository(
    ISqlExecutor sql,
    ILogger<ICourseRepository> logger)
    : ICourseRepository
{
    public async Task<List<Course>?> GetAll()
    {
        const string query = """
                                 SELECT id, title, slug, description, language, created_at, updated_at, tags,
                                        featured_image, lesson_summaries, status
                                 FROM courses
                             """;

        try
        {
            return await sql.QueryAsync<Course>(query) as List<Course>;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all courses");
            throw new Exception("Error retrieving all courses", ex);
        }
    }

    public async Task<Course?> GetById(string courseId)
    {
        const string query = """
                                 SELECT id, title, slug, description, language, created_at, updated_at, tags,
                                    featured_image, lesson_summaries, status
                                    FROM courses
                                 WHERE id = @CourseId
                             """;

        try
        {
            return await sql.QueryFirstOrDefaultAsync<Course>(query, new { CourseId = courseId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving course by ID {CourseId}", courseId);
            throw new Exception($"Error retrieving course by ID {courseId}", ex);
        }
    }

    public async Task<Course?> GetBySlug(string slug)
    {
        const string query = """
                                 SELECT id, title, slug, description, language, created_at, updated_at, tags,
                                    featured_image, lesson_summaries, status
                                    FROM courses
                                 WHERE slug = @Slug
                             """;

        try
        {
            return await sql.QueryFirstOrDefaultAsync<Course>(query, new { Slug = slug });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving course by slug {Slug}", slug);
            throw new Exception($"Error retrieving course by slug {slug}", ex);
        }
    }

    public async Task<Course?> GetByLanguage(string language)
    {
        const string query = """
                                 SELECT id, title, slug, description, language, created_at, updated_at, tags,
                                    featured_image, lesson_summaries, status
                                    FROM courses
                                 WHERE language = @Language
                             """;

        try
        {
            return await sql.QueryFirstOrDefaultAsync<Course>(query, new { Language = language });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving course by language {Language}", language);
            throw new Exception($"Error retrieving course by language {language}", ex);
        }
    }

    public async Task<List<Course>?> GetByTag(string tag)
    {
        const string query = """
                                 SELECT id, title, slug, description, language, created_at, updated_at, tags,
                                        featured_image, lesson_summaries, status
                                 FROM courses
                                 WHERE tags @> @Tag
                             """;

        try
        {
            return await sql.QueryAsync<Course>(query, new { Tag = tag }) as List<Course>;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving course by tag {Tag}", tag);
            throw new Exception($"Error retrieving course by tag {tag}", ex);
        }
    }

    public async Task<List<Course>?> GetByStatus(string status)
    {
        const string query = """
                                 SELECT id, title, slug, description, language, created_at, updated_at, tags,
                                        featured_image, lesson_summaries, status
                                 FROM courses
                                 WHERE status = @Status
                             """;

        try
        {
            return await sql.QueryAsync<Course>(query, new { Status = status }) as List<Course>;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving courses by status {Status}", status);
            throw new Exception($"Error retrieving courses by status {status}", ex);
        }
    }

    public async Task<List<Course>?> GetPublishedCourses()
    {
        const string query = """
                                 SELECT id, title, slug, description, language, created_at, updated_at, tags,
                                        featured_image, lesson_summaries, status
                                 FROM courses
                                 WHERE status = 'published'
                                 ORDER BY created_at DESC
                             """;

        try
        {
            return await sql.QueryAsync<Course>(query) as List<Course>;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving published courses");
            throw new Exception("Error retrieving published courses", ex);
        }
    }

    public async Task<Course> Create(Course course)
    {
        const string query = """
                                 INSERT INTO courses (id, title, slug, description, language, created_at, updated_at,
                                                    tags, featured_image, lesson_summaries, status)
                                 VALUES (@Id, @Title, @Slug, @Description, @Language, @CreatedAt, @UpdatedAt,
                                        @Tags, @FeaturedImage, @LessonSummaries, @Status)
                                 RETURNING id, title, slug, description, language, created_at, updated_at, tags,
                                          featured_image, lesson_summaries, status
                             """;

        try
        {
            course.CreatedAt = DateTime.UtcNow;
            course.UpdatedAt = DateTime.UtcNow;

            return await sql.QueryFirstAsync<Course>(query, course);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating course {Title}", course.Title);
            throw new Exception($"Error creating course {course.Title}", ex);
        }
    }

    public async Task<Course> Update(Course course)
    {
        const string query = """
                                 UPDATE courses
                                 SET title = @Title, slug = @Slug, description = @Description,
                                     language = @Language, updated_at = @UpdatedAt, tags = @Tags,
                                     featured_image = @FeaturedImage, lesson_summaries = @LessonSummaries,
                                     status = @Status
                                 WHERE id = @Id
                                 RETURNING id, title, slug, description, language, created_at, updated_at, tags,
                                          featured_image, lesson_summaries, status
                             """;

        try
        {
            course.UpdatedAt = DateTime.UtcNow;
            return await sql.QueryFirstAsync<Course>(query, course);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating course {Id}", course.Id);
            throw new Exception($"Error updating course {course.Id}", ex);
        }
    }

    public async Task<bool> Delete(string courseId)
    {
        const string query = "DELETE FROM courses WHERE id = @CourseId";

        try
        {
            var rowsAffected = await sql.ExecuteAsync(query, new { CourseId = courseId });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting course {CourseId}", courseId);
            throw new Exception($"Error deleting course {courseId}", ex);
        }
    }

    public async Task<bool> UpdateStatus(string courseId, string status)
    {
        const string query = """
                                 UPDATE courses
                                 SET status = @Status, updated_at = @UpdatedAt
                                 WHERE id = @CourseId
                             """;

        try
        {
            var rowsAffected = await sql.ExecuteAsync(query, new
            {
                CourseId = courseId,
                Status = status,
                UpdatedAt = DateTime.UtcNow
            });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating course status {CourseId}", courseId);
            throw new Exception($"Error updating course status {courseId}", ex);
        }
    }

    public async Task<int> GetTotalCount()
    {
        const string query = "SELECT COUNT(*) FROM courses";

        try
        {
            return await sql.QueryFirstAsync<int>(query);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting total course count");
            throw new Exception("Error getting total course count", ex);
        }
    }

    public async Task<int> GetCountByStatus(string status)
    {
        const string query = "SELECT COUNT(*) FROM courses WHERE status = @Status";

        try
        {
            return await sql.QueryFirstAsync<int>(query, new { Status = status });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting course count by status {Status}", status);
            throw new Exception($"Error getting course count by status {status}", ex);
        }
    }

    public async Task<Dictionary<string, int>> GetCourseStatistics()
    {
        const string query = """
                                 SELECT status, COUNT(*) as count
                                 FROM courses
                                 GROUP BY status
                             """;

        try
        {
            var results = await sql.QueryAsync<dynamic>(query);
            return results.ToDictionary(
                r => (string)r.status,
                r => (int)r.count
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting course statistics");
            throw new Exception("Error getting course statistics", ex);
        }
    }
}