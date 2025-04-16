using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Courses;
using Microsoft.Extensions.Logging;

namespace AscendDev.Data.Repositories;

public class LessonRepository(
    ILogger<ILessonRepository> logger,
    ISqlExecutor sql) : ILessonRepository
{
    public async Task<List<Lesson>?> GetByCourseId(string courseId)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new ArgumentException("Course ID cannot be null or empty", nameof(courseId));

        const string query = """
                                 SELECT id, course_id, title, slug, content, template, created_at, updated_at, language, "order",
                                        additional_resources, tags
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
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Lesson ID cannot be null or empty", nameof(id));

        const string query = """
                                 SELECT id, course_id, title, slug, content, template, created_at, updated_at, language, "order",
                                        additional_resources, tags
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
        if (string.IsNullOrEmpty(slug))
            throw new ArgumentException("Lesson slug cannot be null or empty", nameof(slug));

        const string query = """
                                 SELECT id, course_id, title, slug, content, template, created_at, updated_at, language, "order",
                                        additional_resources, tags
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
}