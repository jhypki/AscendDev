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
                                        featured_image, lesson_summaries, status, created_by
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
        if (string.IsNullOrEmpty(courseId))
            throw new ArgumentException("Course ID cannot be null or empty", nameof(courseId));

        const string query = """
                                 SELECT id, title, slug, description, language, created_at, updated_at, tags,
                                    featured_image, lesson_summaries, status, created_by
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
        if (string.IsNullOrEmpty(slug))
            throw new ArgumentException("Course slug cannot be null or empty", nameof(slug));

        const string query = """
                                 SELECT id, title, slug, description, language, created_at, updated_at, tags,
                                    featured_image, lesson_summaries, status, created_by
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
        if (string.IsNullOrEmpty(language))
            throw new ArgumentException("Course language cannot be null or empty", nameof(language));

        const string query = """
                                 SELECT id, title, slug, description, language, created_at, updated_at, tags,
                                    featured_image, lesson_summaries, status, created_by
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
        if (string.IsNullOrEmpty(tag))
            throw new ArgumentException("Course tag cannot be null or empty", nameof(tag));

        const string query = """
                                 SELECT id, title, slug, description, language, created_at, updated_at, tags
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
}