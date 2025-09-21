using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Courses;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace AscendDev.Data.Repositories;

public class CourseRepository(
    ISqlExecutor sql,
    ILogger<ICourseRepository> logger)
    : ICourseRepository
{
    public async Task<List<Course>?> GetAll()
    {
        const string query = """
                                 SELECT c.id, c.title, c.slug, c.description, c.language, c.created_at, c.updated_at, c.tags,
                                        c.featured_image, c.status,
                                        l.id as Id, l.title as Title, l.slug as Slug, l."order" as Order
                                 FROM courses c
                                 LEFT JOIN lessons l ON c.id = l.course_id
                                 ORDER BY c.created_at DESC, l."order" ASC
                             """;

        try
        {
            var courseDictionary = new Dictionary<string, Course>();

            await sql.QueryAsync<Course, LessonSummary, Course>(query, (course, lesson) =>
            {
                if (!courseDictionary.TryGetValue(course.Id, out var existingCourse))
                {
                    existingCourse = course;
                    existingCourse.LessonSummaries = new List<LessonSummary>();
                    courseDictionary.Add(course.Id, existingCourse);
                }

                if (lesson != null && !string.IsNullOrEmpty(lesson.Id))
                {
                    existingCourse.LessonSummaries.Add(lesson);
                }

                return existingCourse;
            }, splitOn: "Id");

            return courseDictionary.Values.ToList();
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
                                 SELECT c.id, c.title, c.slug, c.description, c.language, c.created_at, c.updated_at, c.tags,
                                        c.featured_image, c.status,
                                        l.id as Id, l.title as Title, l.slug as Slug, l."order" as Order
                                 FROM courses c
                                 LEFT JOIN lessons l ON c.id = l.course_id
                                 WHERE c.id = @CourseId
                                 ORDER BY l."order" ASC
                             """;

        try
        {
            Course? course = null;

            await sql.QueryAsync<Course, LessonSummary, Course>(query, (c, lesson) =>
            {
                if (course == null)
                {
                    course = c;
                    course.LessonSummaries = new List<LessonSummary>();
                }

                if (lesson != null && !string.IsNullOrEmpty(lesson.Id))
                {
                    course.LessonSummaries.Add(lesson);
                }

                return course;
            }, new { CourseId = courseId }, splitOn: "Id");

            return course;
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
                                 SELECT c.id, c.title, c.slug, c.description, c.language, c.created_at, c.updated_at, c.tags,
                                        c.featured_image, c.status,
                                        l.id as Id, l.title as Title, l.slug as Slug, l."order" as Order
                                 FROM courses c
                                 LEFT JOIN lessons l ON c.id = l.course_id
                                 WHERE c.slug = @Slug
                                 ORDER BY l."order" ASC
                             """;

        try
        {
            Course? course = null;

            await sql.QueryAsync<Course, LessonSummary, Course>(query, (c, lesson) =>
            {
                if (course == null)
                {
                    course = c;
                    course.LessonSummaries = new List<LessonSummary>();
                }

                if (lesson != null && !string.IsNullOrEmpty(lesson.Id))
                {
                    course.LessonSummaries.Add(lesson);
                }

                return course;
            }, new { Slug = slug }, splitOn: "Id");

            return course;
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
                                 SELECT c.id, c.title, c.slug, c.description, c.language, c.created_at, c.updated_at, c.tags,
                                        c.featured_image, c.status,
                                        l.id as Id, l.title as Title, l.slug as Slug, l."order" as Order
                                 FROM courses c
                                 LEFT JOIN lessons l ON c.id = l.course_id
                                 WHERE c.language = @Language
                                 ORDER BY l."order" ASC
                             """;

        try
        {
            Course? course = null;

            await sql.QueryAsync<Course, LessonSummary, Course>(query, (c, lesson) =>
            {
                if (course == null)
                {
                    course = c;
                    course.LessonSummaries = new List<LessonSummary>();
                }

                if (lesson != null && !string.IsNullOrEmpty(lesson.Id))
                {
                    course.LessonSummaries.Add(lesson);
                }

                return course;
            }, new { Language = language }, splitOn: "Id");

            return course;
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
                                 SELECT c.id, c.title, c.slug, c.description, c.language, c.created_at, c.updated_at, c.tags,
                                        c.featured_image, c.status,
                                        l.id as Id, l.title as Title, l.slug as Slug, l."order" as Order
                                 FROM courses c
                                 LEFT JOIN lessons l ON c.id = l.course_id
                                 WHERE c.tags @> @Tag
                                 ORDER BY c.created_at DESC, l."order" ASC
                             """;

        try
        {
            var courseDictionary = new Dictionary<string, Course>();

            await sql.QueryAsync<Course, LessonSummary, Course>(query, (course, lesson) =>
            {
                if (!courseDictionary.TryGetValue(course.Id, out var existingCourse))
                {
                    existingCourse = course;
                    existingCourse.LessonSummaries = new List<LessonSummary>();
                    courseDictionary.Add(course.Id, existingCourse);
                }

                if (lesson != null && !string.IsNullOrEmpty(lesson.Id))
                {
                    existingCourse.LessonSummaries.Add(lesson);
                }

                return existingCourse;
            }, new { Tag = tag }, splitOn: "Id");

            return courseDictionary.Values.ToList();
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
                                 SELECT c.id, c.title, c.slug, c.description, c.language, c.created_at, c.updated_at, c.tags,
                                        c.featured_image, c.status,
                                        l.id as Id, l.title as Title, l.slug as Slug, l."order" as Order
                                 FROM courses c
                                 LEFT JOIN lessons l ON c.id = l.course_id
                                 WHERE c.status = @Status
                                 ORDER BY c.created_at DESC, l."order" ASC
                             """;

        try
        {
            var courseDictionary = new Dictionary<string, Course>();

            await sql.QueryAsync<Course, LessonSummary, Course>(query, (course, lesson) =>
            {
                if (!courseDictionary.TryGetValue(course.Id, out var existingCourse))
                {
                    existingCourse = course;
                    existingCourse.LessonSummaries = new List<LessonSummary>();
                    courseDictionary.Add(course.Id, existingCourse);
                }

                if (lesson != null && !string.IsNullOrEmpty(lesson.Id))
                {
                    existingCourse.LessonSummaries.Add(lesson);
                }

                return existingCourse;
            }, new { Status = status }, splitOn: "Id");

            return courseDictionary.Values.ToList();
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
                                 SELECT c.id, c.title, c.slug, c.description, c.language, c.created_at, c.updated_at, c.tags,
                                        c.featured_image, c.status,
                                        l.id as Id, l.title as Title, l.slug as Slug, l."order" as Order
                                 FROM courses c
                                 LEFT JOIN lessons l ON c.id = l.course_id
                                 WHERE c.status = 'published'
                                 ORDER BY c.created_at DESC, l."order" ASC
                             """;

        try
        {
            var courseDictionary = new Dictionary<string, Course>();

            await sql.QueryAsync<Course, LessonSummary, Course>(query, (course, lesson) =>
            {
                if (!courseDictionary.TryGetValue(course.Id, out var existingCourse))
                {
                    existingCourse = course;
                    existingCourse.LessonSummaries = new List<LessonSummary>();
                    courseDictionary.Add(course.Id, existingCourse);
                }

                if (lesson != null && !string.IsNullOrEmpty(lesson.Id))
                {
                    existingCourse.LessonSummaries.Add(lesson);
                }

                return existingCourse;
            }, splitOn: "Id");

            return courseDictionary.Values.ToList();
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
                                                    tags, featured_image, status)
                                 VALUES (@Id, @Title, @Slug, @Description, @Language, @CreatedAt, @UpdatedAt,
                                        @Tags, @FeaturedImage, @Status)
                                 RETURNING id, title, slug, description, language, created_at, updated_at, tags,
                                          featured_image, status
                             """;

        try
        {
            course.CreatedAt = DateTime.UtcNow;
            course.UpdatedAt = DateTime.UtcNow;

            var createdCourse = await sql.QueryFirstAsync<Course>(query, course);
            createdCourse.LessonSummaries = new List<LessonSummary>();
            return createdCourse;
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
                                     featured_image = @FeaturedImage,
                                     status = @Status
                                 WHERE id = @Id
                                 RETURNING id, title, slug, description, language, created_at, updated_at, tags,
                                          featured_image, status
                             """;

        try
        {
            course.UpdatedAt = DateTime.UtcNow;
            var createdCourse = await sql.QueryFirstAsync<Course>(query, course);
            createdCourse.LessonSummaries = new List<LessonSummary>();
            return createdCourse;
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

    // Extended methods for analytics
    public async Task<int> CountAsync()
    {
        const string query = "SELECT COUNT(*) FROM courses";
        try
        {
            return await sql.QueryFirstAsync<int>(query);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error counting courses");
            throw new Exception("Error counting courses", ex);
        }
    }

    public async Task<int> CountAsync(Expression<Func<Course, bool>> predicate)
    {
        // Simplified implementation for common cases
        if (predicate.Body is BinaryExpression binaryExpr)
        {
            // Handle status comparison
            if (binaryExpr.Left is MemberExpression memberExpr && memberExpr.Member.Name == "Status" &&
                binaryExpr.Right is ConstantExpression constantExpr && constantExpr.Value is string status)
            {
                const string query = "SELECT COUNT(*) FROM courses WHERE status = @Status";
                try
                {
                    return await sql.QueryFirstAsync<int>(query, new { Status = status });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error counting courses by status {Status}", status);
                    throw new Exception($"Error counting courses by status {status}", ex);
                }
            }

            // Handle date comparison for CreatedAt
            if (binaryExpr.Left is MemberExpression dateMember && dateMember.Member.Name == "CreatedAt" &&
                binaryExpr.NodeType == ExpressionType.GreaterThanOrEqual &&
                binaryExpr.Right is ConstantExpression dateConstant && dateConstant.Value is DateTime date)
            {
                const string query = "SELECT COUNT(*) FROM courses WHERE created_at >= @Date AND created_at < @EndDate";
                try
                {
                    return await sql.QueryFirstAsync<int>(query, new { Date = date, EndDate = date.AddDays(1) });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error counting courses by date");
                    throw new Exception("Error counting courses by date", ex);
                }
            }
        }

        // Fallback - get all courses and filter in memory
        logger.LogWarning("Using inefficient in-memory filtering for course count");
        var allCourses = await GetAll();
        return allCourses?.Where(predicate.Compile()).Count() ?? 0;
    }

    public IQueryable<Course> GetQueryable()
    {
        // Simplified implementation - return empty queryable
        // In a real EF Core scenario, this would return the DbSet
        return new List<Course>().AsQueryable();
    }
}