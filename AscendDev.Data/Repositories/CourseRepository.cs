using AscendDev.Core.DTOs.Courses;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Courses;
using Microsoft.Extensions.Logging;

namespace AscendDev.Data.Repositories;

public class CourseRepository(
    ISqlExecutor sql,
    ILogger<ICourseRepository> logger)
    : ICourseRepository
{
    private const string BaseSelectQuery = """
        SELECT id, title, slug, description, language, created_at, updated_at, tags,
               featured_image, lesson_summaries, status, version, parent_course_id,
               is_published, published_at, published_by, created_by, updated_by,
               view_count, enrollment_count, rating, rating_count, is_validated,
               validated_at, validated_by, validation_errors
        FROM courses
        """;

    #region Read Operations

    public async Task<List<Course>?> GetAll()
    {
        try
        {
            return await sql.QueryAsync<Course>(BaseSelectQuery) as List<Course>;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all courses");
            throw new Exception("Error retrieving all courses", ex);
        }
    }

    public async Task<Course?> GetById(string courseId)
    {
        const string query = BaseSelectQuery + " WHERE id = @CourseId";

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
        const string query = BaseSelectQuery + " WHERE slug = @Slug";

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
        const string query = BaseSelectQuery + " WHERE language = @Language";

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
        const string query = BaseSelectQuery + " WHERE tags @> @Tag";

        try
        {
            return await sql.QueryAsync<Course>(query, new { Tag = $"[\"{tag}\"]" }) as List<Course>;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving course by tag {Tag}", tag);
            throw new Exception($"Error retrieving course by tag {tag}", ex);
        }
    }

    public async Task<List<Course>?> GetByStatus(string status)
    {
        const string query = BaseSelectQuery + " WHERE status = @Status";

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

    public async Task<List<Course>?> GetPublished()
    {
        const string query = BaseSelectQuery + " WHERE is_published = true AND status = 'published'";

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

    public async Task<List<Course>?> GetByCreator(string creatorId)
    {
        const string query = BaseSelectQuery + " WHERE created_by = @CreatorId";

        try
        {
            return await sql.QueryAsync<Course>(query, new { CreatorId = creatorId }) as List<Course>;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving courses by creator {CreatorId}", creatorId);
            throw new Exception($"Error retrieving courses by creator {creatorId}", ex);
        }
    }

    #endregion

    #region CRUD Operations

    public async Task<Course> Create(Course course)
    {
        const string query = """
            INSERT INTO courses (id, title, slug, description, language, created_at, updated_at, tags,
                               featured_image, lesson_summaries, status, version, parent_course_id,
                               is_published, published_at, published_by, created_by, updated_by,
                               view_count, enrollment_count, rating, rating_count, is_validated,
                               validated_at, validated_by, validation_errors)
            VALUES (@Id, @Title, @Slug, @Description, @Language, @CreatedAt, @UpdatedAt, @Tags,
                   @FeaturedImage, @LessonSummaries, @Status, @Version, @ParentCourseId,
                   @IsPublished, @PublishedAt, @PublishedBy, @CreatedBy, @UpdatedBy,
                   @ViewCount, @EnrollmentCount, @Rating, @RatingCount, @IsValidated,
                   @ValidatedAt, @ValidatedBy, @ValidationErrors)
            RETURNING *
            """;

        try
        {
            course.Id = Guid.NewGuid().ToString();
            course.CreatedAt = DateTime.UtcNow;
            course.UpdatedAt = DateTime.UtcNow;

            // Prepare parameters with proper UUID handling
            var parameters = new
            {
                course.Id,
                course.Title,
                course.Slug,
                course.Description,
                course.Language,
                course.CreatedAt,
                course.UpdatedAt,
                course.Tags,
                course.FeaturedImage,
                course.LessonSummaries,
                course.Status,
                course.Version,
                course.ParentCourseId,
                course.IsPublished,
                course.PublishedAt,
                PublishedBy = course.PublishedBy,
                CreatedBy = course.CreatedBy,
                UpdatedBy = course.UpdatedBy,
                course.ViewCount,
                course.EnrollmentCount,
                course.Rating,
                course.RatingCount,
                course.IsValidated,
                course.ValidatedAt,
                ValidatedBy = course.ValidatedBy,
                course.ValidationErrors
            };

            var result = await sql.QueryFirstOrDefaultAsync<Course>(query, parameters);
            return result ?? throw new Exception("Failed to create course");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating course {Title}", course.Title);
            throw new Exception($"Error creating course {course.Title}", ex);
        }
    }

    public async Task<Course?> Update(string courseId, Course course)
    {
        const string query = """
            UPDATE courses
            SET title = @Title, slug = @Slug, description = @Description, language = @Language,
                updated_at = @UpdatedAt, tags = @Tags, featured_image = @FeaturedImage,
                lesson_summaries = @LessonSummaries, status = @Status, updated_by = @UpdatedBy,
                validation_errors = @ValidationErrors
            WHERE id = @Id
            RETURNING *
            """;

        try
        {
            course.Id = courseId;
            course.UpdatedAt = DateTime.UtcNow;

            return await sql.QueryFirstOrDefaultAsync<Course>(query, course);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating course {CourseId}", courseId);
            throw new Exception($"Error updating course {courseId}", ex);
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

    public async Task<bool> SoftDelete(string courseId)
    {
        const string query = """
            UPDATE courses
            SET status = 'deleted', updated_at = @UpdatedAt
            WHERE id = @CourseId
            """;

        try
        {
            var rowsAffected = await sql.ExecuteAsync(query, new { CourseId = courseId, UpdatedAt = DateTime.UtcNow });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error soft deleting course {CourseId}", courseId);
            throw new Exception($"Error soft deleting course {courseId}", ex);
        }
    }

    #endregion

    #region Versioning Operations

    public async Task<Course> CreateVersion(string courseId, string userId)
    {
        try
        {
            var originalCourse = await GetById(courseId);
            if (originalCourse == null)
                throw new Exception($"Course {courseId} not found");

            var newVersion = new Course
            {
                Id = Guid.NewGuid().ToString(),
                Title = originalCourse.Title,
                Slug = originalCourse.Slug + "-v" + (originalCourse.Version + 1),
                Description = originalCourse.Description,
                Language = originalCourse.Language,
                FeaturedImage = originalCourse.FeaturedImage,
                Tags = originalCourse.Tags,
                LessonSummaries = originalCourse.LessonSummaries,
                Status = "draft",
                Version = originalCourse.Version + 1,
                ParentCourseId = originalCourse.ParentCourseId ?? originalCourse.Id,
                CreatedBy = Guid.Parse(userId),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return await Create(newVersion);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating version for course {CourseId}", courseId);
            throw new Exception($"Error creating version for course {courseId}", ex);
        }
    }

    public async Task<List<Course>?> GetVersions(string parentCourseId)
    {
        const string query = BaseSelectQuery + " WHERE parent_course_id = @ParentCourseId OR id = @ParentCourseId ORDER BY version";

        try
        {
            return await sql.QueryAsync<Course>(query, new { ParentCourseId = parentCourseId }) as List<Course>;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving versions for course {ParentCourseId}", parentCourseId);
            throw new Exception($"Error retrieving versions for course {parentCourseId}", ex);
        }
    }

    public async Task<Course?> GetLatestVersion(string parentCourseId)
    {
        const string query = BaseSelectQuery + " WHERE parent_course_id = @ParentCourseId OR id = @ParentCourseId ORDER BY version DESC LIMIT 1";

        try
        {
            return await sql.QueryFirstOrDefaultAsync<Course>(query, new { ParentCourseId = parentCourseId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving latest version for course {ParentCourseId}", parentCourseId);
            throw new Exception($"Error retrieving latest version for course {parentCourseId}", ex);
        }
    }

    #endregion

    #region Publishing Operations

    public async Task<bool> Publish(string courseId, string userId)
    {
        const string query = """
            UPDATE courses
            SET is_published = true, published_at = @PublishedAt, published_by = @PublishedBy,
                status = 'published', updated_at = @UpdatedAt
            WHERE id = @CourseId
            """;

        try
        {
            var rowsAffected = await sql.ExecuteAsync(query, new
            {
                CourseId = courseId,
                PublishedAt = DateTime.UtcNow,
                PublishedBy = Guid.Parse(userId),
                UpdatedAt = DateTime.UtcNow
            });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing course {CourseId}", courseId);
            throw new Exception($"Error publishing course {courseId}", ex);
        }
    }

    public async Task<bool> Unpublish(string courseId, string userId)
    {
        const string query = """
            UPDATE courses
            SET is_published = false, status = 'draft', updated_at = @UpdatedAt, updated_by = @UpdatedBy
            WHERE id = @CourseId
            """;

        try
        {
            var rowsAffected = await sql.ExecuteAsync(query, new
            {
                CourseId = courseId,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = Guid.Parse(userId)
            });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error unpublishing course {CourseId}", courseId);
            throw new Exception($"Error unpublishing course {courseId}", ex);
        }
    }

    #endregion

    #region Validation Operations

    public async Task<bool> Validate(string courseId, string userId, List<string> validationErrors)
    {
        const string query = """
            UPDATE courses
            SET validation_errors = @ValidationErrors, updated_at = @UpdatedAt, updated_by = @UpdatedBy
            WHERE id = @CourseId
            """;

        try
        {
            var rowsAffected = await sql.ExecuteAsync(query, new
            {
                CourseId = courseId,
                ValidationErrors = validationErrors,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = Guid.Parse(userId)
            });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating course {CourseId}", courseId);
            throw new Exception($"Error validating course {courseId}", ex);
        }
    }

    public async Task<bool> MarkAsValidated(string courseId, string userId)
    {
        const string query = """
            UPDATE courses
            SET is_validated = true, validated_at = @ValidatedAt, validated_by = @ValidatedBy,
                validation_errors = '[]', updated_at = @UpdatedAt
            WHERE id = @CourseId
            """;

        try
        {
            var rowsAffected = await sql.ExecuteAsync(query, new
            {
                CourseId = courseId,
                ValidatedAt = DateTime.UtcNow,
                ValidatedBy = Guid.Parse(userId),
                UpdatedAt = DateTime.UtcNow
            });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error marking course as validated {CourseId}", courseId);
            throw new Exception($"Error marking course as validated {courseId}", ex);
        }
    }

    #endregion

    #region Analytics Operations

    public async Task<CourseAnalyticsResponse?> GetAnalytics(string courseId)
    {
        const string query = """
            SELECT
                c.id as CourseId,
                c.title as Title,
                c.enrollment_count as TotalEnrollments,
                c.view_count as TotalViews,
                COALESCE(active_students.count, 0) as ActiveStudents,
                COALESCE(completed_students.count, 0) as CompletedStudents,
                CASE
                    WHEN c.enrollment_count > 0 THEN COALESCE(completed_students.count, 0)::float / c.enrollment_count * 100
                    ELSE 0
                END as CompletionRate,
                COALESCE(avg_progress.average, 0) as AverageProgress,
                COALESCE(lesson_count.count, 0) as TotalLessons,
                c.updated_at as LastAccessed
            FROM courses c
            LEFT JOIN (
                SELECT course_id, COUNT(*) as count
                FROM user_progress
                WHERE status = 'in_progress' AND course_id = @CourseId
                GROUP BY course_id
            ) active_students ON c.id = active_students.course_id
            LEFT JOIN (
                SELECT course_id, COUNT(*) as count
                FROM user_progress
                WHERE status = 'completed' AND course_id = @CourseId
                GROUP BY course_id
            ) completed_students ON c.id = completed_students.course_id
            LEFT JOIN (
                SELECT course_id, AVG(progress_percentage) as average
                FROM user_progress
                WHERE course_id = @CourseId
                GROUP BY course_id
            ) avg_progress ON c.id = avg_progress.course_id
            LEFT JOIN (
                SELECT course_id, COUNT(*) as count
                FROM lessons
                WHERE course_id = @CourseId
                GROUP BY course_id
            ) lesson_count ON c.id = lesson_count.course_id
            WHERE c.id = @CourseId
            """;

        try
        {
            return await sql.QueryFirstOrDefaultAsync<CourseAnalyticsResponse>(query, new { CourseId = courseId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving analytics for course {CourseId}", courseId);
            throw new Exception($"Error retrieving analytics for course {courseId}", ex);
        }
    }

    public async Task<bool> IncrementViewCount(string courseId)
    {
        const string query = """
            UPDATE courses
            SET view_count = view_count + 1, updated_at = @UpdatedAt
            WHERE id = @CourseId
            """;

        try
        {
            var rowsAffected = await sql.ExecuteAsync(query, new { CourseId = courseId, UpdatedAt = DateTime.UtcNow });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error incrementing view count for course {CourseId}", courseId);
            throw new Exception($"Error incrementing view count for course {courseId}", ex);
        }
    }

    public async Task<bool> IncrementEnrollmentCount(string courseId)
    {
        const string query = """
            UPDATE courses
            SET enrollment_count = enrollment_count + 1, updated_at = @UpdatedAt
            WHERE id = @CourseId
            """;

        try
        {
            var rowsAffected = await sql.ExecuteAsync(query, new { CourseId = courseId, UpdatedAt = DateTime.UtcNow });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error incrementing enrollment count for course {CourseId}", courseId);
            throw new Exception($"Error incrementing enrollment count for course {courseId}", ex);
        }
    }

    public async Task<bool> UpdateRating(string courseId, double rating, int ratingCount)
    {
        const string query = """
            UPDATE courses
            SET rating = @Rating, rating_count = @RatingCount, updated_at = @UpdatedAt
            WHERE id = @CourseId
            """;

        try
        {
            var rowsAffected = await sql.ExecuteAsync(query, new
            {
                CourseId = courseId,
                Rating = rating,
                RatingCount = ratingCount,
                UpdatedAt = DateTime.UtcNow
            });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating rating for course {CourseId}", courseId);
            throw new Exception($"Error updating rating for course {courseId}", ex);
        }
    }

    #endregion

    #region Search and Filtering

    public async Task<List<Course>?> Search(string searchTerm, string? language = null, List<string>? tags = null, string? status = null)
    {
        var whereConditions = new List<string>();
        var parameters = new Dictionary<string, object>();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            whereConditions.Add("(title ILIKE @SearchTerm OR description ILIKE @SearchTerm)");
            parameters.Add("SearchTerm", $"%{searchTerm}%");
        }

        if (!string.IsNullOrEmpty(language))
        {
            whereConditions.Add("language = @Language");
            parameters.Add("Language", language);
        }

        if (tags != null && tags.Count > 0)
        {
            whereConditions.Add("tags && @Tags");
            parameters.Add("Tags", tags.ToArray());
        }

        if (!string.IsNullOrEmpty(status))
        {
            whereConditions.Add("status = @Status");
            parameters.Add("Status", status);
        }

        var whereClause = whereConditions.Count > 0 ? " WHERE " + string.Join(" AND ", whereConditions) : "";
        var query = BaseSelectQuery + whereClause;

        try
        {
            return await sql.QueryAsync<Course>(query, parameters) as List<Course>;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching courses with term {SearchTerm}", searchTerm);
            throw new Exception($"Error searching courses with term {searchTerm}", ex);
        }
    }

    public async Task<List<Course>?> GetPaginated(int page, int pageSize, string? sortBy = null, bool ascending = true)
    {
        var orderBy = sortBy switch
        {
            "title" => "title",
            "created_at" => "created_at",
            "updated_at" => "updated_at",
            "rating" => "rating",
            "view_count" => "view_count",
            _ => "created_at"
        };

        var direction = ascending ? "ASC" : "DESC";
        var offset = (page - 1) * pageSize;

        var query = $"{BaseSelectQuery} ORDER BY {orderBy} {direction} LIMIT @PageSize OFFSET @Offset";

        try
        {
            return await sql.QueryAsync<Course>(query, new { PageSize = pageSize, Offset = offset }) as List<Course>;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving paginated courses");
            throw new Exception("Error retrieving paginated courses", ex);
        }
    }

    #endregion
}