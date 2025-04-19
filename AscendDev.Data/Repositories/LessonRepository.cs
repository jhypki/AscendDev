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
            logger.LogInformation("Retrieving lesson by ID {Id}", id);
            //log lesson
            logger.LogInformation("Lesson: {Lesson}", sql.QueryFirstOrDefaultAsync<Lesson>(query, new { Id = id }));
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
}