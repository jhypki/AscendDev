using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Courses;
using Microsoft.Extensions.Logging;

namespace AscendDev.Data.Repositories;

public class LessonRepository(
    ILogger<ILessonRepository> logger,
    ISqlExecutor sql) : ILessonRepository
{
    private const string BaseSelectQuery = """
        SELECT id, course_id, title, slug, content, template, created_at, updated_at, language, "order",
               test_config, additional_resources, tags, status, created_by, updated_by,
               view_count, completion_count, average_time_spent, is_validated, validated_at,
               validated_by, validation_errors, is_published, published_at, published_by,
               difficulty, estimated_time_minutes, prerequisites
        FROM lessons
        """;

    #region Read Operations

    public async Task<List<Lesson>?> GetByCourseId(string courseId)
    {
        const string query = BaseSelectQuery + " WHERE course_id = @CourseId ORDER BY \"order\"";

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
        const string query = BaseSelectQuery + " WHERE id = @Id";

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
        const string query = BaseSelectQuery + " WHERE slug = @Slug";

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
        const string query = BaseSelectQuery + " WHERE status = @Status ORDER BY created_at DESC";

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

    public async Task<List<Lesson>?> GetPublished()
    {
        const string query = BaseSelectQuery + " WHERE is_published = true AND status = 'published' ORDER BY published_at DESC";

        try
        {
            return await sql.QueryAsync<Lesson>(query) as List<Lesson>;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving published lessons");
            throw new Exception("Error retrieving published lessons", ex);
        }
    }

    public async Task<List<Lesson>?> GetByCreator(string creatorId)
    {
        const string query = BaseSelectQuery + " WHERE created_by = @CreatorId ORDER BY created_at DESC";

        try
        {
            return await sql.QueryAsync<Lesson>(query, new { CreatorId = creatorId }) as List<Lesson>;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving lessons by creator {CreatorId}", creatorId);
            throw new Exception($"Error retrieving lessons by creator {creatorId}", ex);
        }
    }

    public async Task<List<Lesson>?> GetByDifficulty(string difficulty)
    {
        const string query = BaseSelectQuery + " WHERE difficulty = @Difficulty ORDER BY \"order\"";

        try
        {
            return await sql.QueryAsync<Lesson>(query, new { Difficulty = difficulty }) as List<Lesson>;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving lessons by difficulty {Difficulty}", difficulty);
            throw new Exception($"Error retrieving lessons by difficulty {difficulty}", ex);
        }
    }

    #endregion

    #region CRUD Operations

    public async Task<Lesson> Create(Lesson lesson)
    {
        const string query = """
            INSERT INTO lessons (id, course_id, title, slug, content, template, created_at, updated_at,
                               language, "order", test_config, additional_resources, tags, status,
                               created_by, updated_by, view_count, completion_count, average_time_spent,
                               is_validated, validated_at, validated_by, validation_errors, is_published,
                               published_at, published_by, difficulty, estimated_time_minutes, prerequisites)
            VALUES (@Id, @CourseId, @Title, @Slug, @Content, @Template, @CreatedAt, @UpdatedAt,
                   @Language, @Order, @TestConfig, @AdditionalResources, @Tags, @Status,
                   @CreatedBy, @UpdatedBy, @ViewCount, @CompletionCount, @AverageTimeSpent,
                   @IsValidated, @ValidatedAt, @ValidatedBy, @ValidationErrors, @IsPublished,
                   @PublishedAt, @PublishedBy, @Difficulty, @EstimatedTimeMinutes, @Prerequisites)
            RETURNING *
            """;

        try
        {
            lesson.Id = Guid.NewGuid().ToString();
            lesson.CreatedAt = DateTime.UtcNow;
            lesson.UpdatedAt = DateTime.UtcNow;

            // Prepare parameters with proper UUID handling
            var parameters = new
            {
                lesson.Id,
                lesson.CourseId,
                lesson.Title,
                lesson.Slug,
                lesson.Content,
                lesson.Template,
                lesson.CreatedAt,
                lesson.UpdatedAt,
                lesson.Language,
                lesson.Order,
                lesson.TestConfig,
                lesson.AdditionalResources,
                lesson.Tags,
                lesson.Status,
                CreatedBy = lesson.CreatedBy,
                UpdatedBy = lesson.UpdatedBy,
                lesson.ViewCount,
                lesson.CompletionCount,
                lesson.AverageTimeSpent,
                lesson.IsValidated,
                lesson.ValidatedAt,
                ValidatedBy = lesson.ValidatedBy,
                lesson.ValidationErrors,
                lesson.IsPublished,
                lesson.PublishedAt,
                PublishedBy = lesson.PublishedBy,
                lesson.Difficulty,
                lesson.EstimatedTimeMinutes,
                lesson.Prerequisites
            };

            var result = await sql.QueryFirstOrDefaultAsync<Lesson>(query, parameters);
            return result ?? throw new Exception("Failed to create lesson");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating lesson {Title}", lesson.Title);
            throw new Exception($"Error creating lesson {lesson.Title}", ex);
        }
    }

    public async Task<Lesson?> Update(string lessonId, Lesson lesson)
    {
        const string query = """
            UPDATE lessons
            SET title = @Title, slug = @Slug, content = @Content, template = @Template,
                updated_at = @UpdatedAt, language = @Language, "order" = @Order,
                test_config = @TestConfig, additional_resources = @AdditionalResources,
                tags = @Tags, status = @Status, updated_by = @UpdatedBy,
                difficulty = @Difficulty, estimated_time_minutes = @EstimatedTimeMinutes,
                prerequisites = @Prerequisites, validation_errors = @ValidationErrors
            WHERE id = @Id
            RETURNING *
            """;

        try
        {
            lesson.Id = lessonId;
            lesson.UpdatedAt = DateTime.UtcNow;

            return await sql.QueryFirstOrDefaultAsync<Lesson>(query, lesson);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating lesson {LessonId}", lessonId);
            throw new Exception($"Error updating lesson {lessonId}", ex);
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

    public async Task<bool> SoftDelete(string lessonId)
    {
        const string query = """
            UPDATE lessons
            SET status = 'deleted', updated_at = @UpdatedAt
            WHERE id = @LessonId
            """;

        try
        {
            var rowsAffected = await sql.ExecuteAsync(query, new { LessonId = lessonId, UpdatedAt = DateTime.UtcNow });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error soft deleting lesson {LessonId}", lessonId);
            throw new Exception($"Error soft deleting lesson {lessonId}", ex);
        }
    }

    #endregion

    #region Ordering Operations

    public async Task<bool> ReorderLessons(string courseId, List<string> lessonIds)
    {
        const string query = """
            UPDATE lessons
            SET "order" = @Order, updated_at = @UpdatedAt
            WHERE id = @LessonId AND course_id = @CourseId
            """;

        try
        {
            var parameters = lessonIds.Select((lessonId, index) => new
            {
                LessonId = lessonId,
                CourseId = courseId,
                Order = index + 1,
                UpdatedAt = DateTime.UtcNow
            }).ToList();

            var totalRowsAffected = 0;
            foreach (var param in parameters)
            {
                var rowsAffected = await sql.ExecuteAsync(query, param);
                totalRowsAffected += rowsAffected;
            }

            return totalRowsAffected == lessonIds.Count;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reordering lessons for course {CourseId}", courseId);
            throw new Exception($"Error reordering lessons for course {courseId}", ex);
        }
    }

    public async Task<List<Lesson>?> GetOrderedByCourseId(string courseId)
    {
        const string query = BaseSelectQuery + " WHERE course_id = @CourseId ORDER BY \"order\", created_at";

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

    public async Task<int> GetNextOrderNumber(string courseId)
    {
        const string query = """
            SELECT COALESCE(MAX("order"), 0) + 1
            FROM lessons
            WHERE course_id = @CourseId
            """;

        try
        {
            return await sql.QueryFirstOrDefaultAsync<int>(query, new { CourseId = courseId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting next order number for course {CourseId}", courseId);
            throw new Exception($"Error getting next order number for course {courseId}", ex);
        }
    }

    #endregion

    #region Publishing Operations

    public async Task<bool> Publish(string lessonId, string userId)
    {
        const string query = """
            UPDATE lessons
            SET is_published = true, published_at = @PublishedAt, published_by = @PublishedBy,
                status = 'published', updated_at = @UpdatedAt
            WHERE id = @LessonId
            """;

        try
        {
            var rowsAffected = await sql.ExecuteAsync(query, new
            {
                LessonId = lessonId,
                PublishedAt = DateTime.UtcNow,
                PublishedBy = Guid.Parse(userId),
                UpdatedAt = DateTime.UtcNow
            });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing lesson {LessonId}", lessonId);
            throw new Exception($"Error publishing lesson {lessonId}", ex);
        }
    }

    public async Task<bool> Unpublish(string lessonId, string userId)
    {
        const string query = """
            UPDATE lessons
            SET is_published = false, status = 'draft', updated_at = @UpdatedAt, updated_by = @UpdatedBy
            WHERE id = @LessonId
            """;

        try
        {
            var rowsAffected = await sql.ExecuteAsync(query, new
            {
                LessonId = lessonId,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = Guid.Parse(userId)
            });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error unpublishing lesson {LessonId}", lessonId);
            throw new Exception($"Error unpublishing lesson {lessonId}", ex);
        }
    }

    #endregion

    #region Validation Operations

    public async Task<bool> Validate(string lessonId, string userId, List<string> validationErrors)
    {
        const string query = """
            UPDATE lessons
            SET validation_errors = @ValidationErrors, updated_at = @UpdatedAt, updated_by = @UpdatedBy
            WHERE id = @LessonId
            """;

        try
        {
            var rowsAffected = await sql.ExecuteAsync(query, new
            {
                LessonId = lessonId,
                ValidationErrors = validationErrors,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = Guid.Parse(userId)
            });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating lesson {LessonId}", lessonId);
            throw new Exception($"Error validating lesson {lessonId}", ex);
        }
    }

    public async Task<bool> MarkAsValidated(string lessonId, string userId)
    {
        const string query = """
            UPDATE lessons
            SET is_validated = true, validated_at = @ValidatedAt, validated_by = @ValidatedBy,
                validation_errors = '[]', updated_at = @UpdatedAt
            WHERE id = @LessonId
            """;

        try
        {
            var rowsAffected = await sql.ExecuteAsync(query, new
            {
                LessonId = lessonId,
                ValidatedAt = DateTime.UtcNow,
                ValidatedBy = Guid.Parse(userId),
                UpdatedAt = DateTime.UtcNow
            });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error marking lesson as validated {LessonId}", lessonId);
            throw new Exception($"Error marking lesson as validated {lessonId}", ex);
        }
    }

    #endregion

    #region Analytics Operations

    public async Task<bool> IncrementViewCount(string lessonId)
    {
        const string query = """
            UPDATE lessons
            SET view_count = view_count + 1, updated_at = @UpdatedAt
            WHERE id = @LessonId
            """;

        try
        {
            var rowsAffected = await sql.ExecuteAsync(query, new { LessonId = lessonId, UpdatedAt = DateTime.UtcNow });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error incrementing view count for lesson {LessonId}", lessonId);
            throw new Exception($"Error incrementing view count for lesson {lessonId}", ex);
        }
    }

    public async Task<bool> IncrementCompletionCount(string lessonId)
    {
        const string query = """
            UPDATE lessons
            SET completion_count = completion_count + 1, updated_at = @UpdatedAt
            WHERE id = @LessonId
            """;

        try
        {
            var rowsAffected = await sql.ExecuteAsync(query, new { LessonId = lessonId, UpdatedAt = DateTime.UtcNow });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error incrementing completion count for lesson {LessonId}", lessonId);
            throw new Exception($"Error incrementing completion count for lesson {lessonId}", ex);
        }
    }

    public async Task<bool> UpdateAverageTimeSpent(string lessonId, double timeSpent)
    {
        const string query = """
            UPDATE lessons
            SET average_time_spent = (average_time_spent * completion_count + @TimeSpent) / (completion_count + 1),
                updated_at = @UpdatedAt
            WHERE id = @LessonId
            """;

        try
        {
            var rowsAffected = await sql.ExecuteAsync(query, new
            {
                LessonId = lessonId,
                TimeSpent = timeSpent,
                UpdatedAt = DateTime.UtcNow
            });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating average time spent for lesson {LessonId}", lessonId);
            throw new Exception($"Error updating average time spent for lesson {lessonId}", ex);
        }
    }

    #endregion

    #region Search and Filtering

    public async Task<List<Lesson>?> Search(string searchTerm, string? courseId = null, string? difficulty = null, List<string>? tags = null)
    {
        var whereConditions = new List<string>();
        var parameters = new Dictionary<string, object>();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            whereConditions.Add("(title ILIKE @SearchTerm OR content ILIKE @SearchTerm)");
            parameters.Add("SearchTerm", $"%{searchTerm}%");
        }

        if (!string.IsNullOrEmpty(courseId))
        {
            whereConditions.Add("course_id = @CourseId");
            parameters.Add("CourseId", courseId);
        }

        if (!string.IsNullOrEmpty(difficulty))
        {
            whereConditions.Add("difficulty = @Difficulty");
            parameters.Add("Difficulty", difficulty);
        }

        if (tags != null && tags.Count > 0)
        {
            whereConditions.Add("tags && @Tags");
            parameters.Add("Tags", tags.ToArray());
        }

        var whereClause = whereConditions.Count > 0 ? " WHERE " + string.Join(" AND ", whereConditions) : "";
        var query = BaseSelectQuery + whereClause + " ORDER BY \"order\"";

        try
        {
            return await sql.QueryAsync<Lesson>(query, parameters) as List<Lesson>;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching lessons with term {SearchTerm}", searchTerm);
            throw new Exception($"Error searching lessons with term {searchTerm}", ex);
        }
    }

    public async Task<List<Lesson>?> GetPaginated(int page, int pageSize, string? courseId = null, string? sortBy = null, bool ascending = true)
    {
        var whereClause = !string.IsNullOrEmpty(courseId) ? " WHERE course_id = @CourseId" : "";

        var orderBy = sortBy switch
        {
            "title" => "title",
            "created_at" => "created_at",
            "updated_at" => "updated_at",
            "order" => "\"order\"",
            "difficulty" => "difficulty",
            _ => "\"order\""
        };

        var direction = ascending ? "ASC" : "DESC";
        var offset = (page - 1) * pageSize;

        var query = $"{BaseSelectQuery}{whereClause} ORDER BY {orderBy} {direction} LIMIT @PageSize OFFSET @Offset";

        var parameters = new Dictionary<string, object>
        {
            { "PageSize", pageSize },
            { "Offset", offset }
        };

        if (!string.IsNullOrEmpty(courseId))
        {
            parameters.Add("CourseId", courseId);
        }

        try
        {
            return await sql.QueryAsync<Lesson>(query, parameters) as List<Lesson>;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving paginated lessons");
            throw new Exception("Error retrieving paginated lessons", ex);
        }
    }

    #endregion

    #region Prerequisites

    public async Task<List<Lesson>?> GetPrerequisites(string lessonId)
    {
        const string query = """
            SELECT l.* FROM lessons l
            INNER JOIN (
                SELECT unnest(prerequisites) as prereq_id
                FROM lessons
                WHERE id = @LessonId
            ) p ON l.id = p.prereq_id
            ORDER BY l."order"
            """;

        try
        {
            return await sql.QueryAsync<Lesson>(query, new { LessonId = lessonId }) as List<Lesson>;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving prerequisites for lesson {LessonId}", lessonId);
            throw new Exception($"Error retrieving prerequisites for lesson {lessonId}", ex);
        }
    }

    public async Task<bool> HasCompletedPrerequisites(string lessonId, string userId)
    {
        const string query = """
            SELECT COUNT(*) = 0 FROM (
                SELECT unnest(prerequisites) as prereq_id
                FROM lessons
                WHERE id = @LessonId
            ) p
            LEFT JOIN user_progress up ON p.prereq_id = up.lesson_id AND up.user_id = @UserId
            WHERE up.status != 'completed' OR up.status IS NULL
            """;

        try
        {
            return await sql.QueryFirstOrDefaultAsync<bool>(query, new { LessonId = lessonId, UserId = Guid.Parse(userId) });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking prerequisites completion for lesson {LessonId} and user {UserId}", lessonId, userId);
            throw new Exception($"Error checking prerequisites completion for lesson {lessonId} and user {userId}", ex);
        }
    }

    #endregion
}