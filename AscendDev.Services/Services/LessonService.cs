using AscendDev.Core.Caching;
using AscendDev.Core.DTOs.Courses;
using AscendDev.Core.Exceptions;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.Courses;
using Microsoft.Extensions.Logging;

namespace AscendDev.Services.Services;

public class LessonService(
    ILessonRepository lessonRepository,
    ICourseRepository courseRepository,
    ILogger<ILessonService> logger,
    ICachingService cachingService) : ILessonService
{
    #region Read Operations

    public async Task<List<LessonResponse>> GetLessonsByCourseId(string courseId)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        var cacheKey = CacheKeys.LessonsByCourseId(courseId);

        try
        {
            var lessons = await cachingService.GetOrCreateAsync(cacheKey, () => lessonRepository.GetByCourseId(courseId));
            if (lessons == null || lessons.Count == 0)
                return new List<LessonResponse>();

            return lessons.Select(MapToResponse).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching lessons for course {CourseId}", courseId);
            throw;
        }
    }

    public async Task<LessonResponse> GetLessonById(string lessonId)
    {
        if (string.IsNullOrEmpty(lessonId))
            throw new BadRequestException("Lesson ID cannot be null or empty");

        var cacheKey = CacheKeys.LessonById(lessonId);

        try
        {
            var lesson = await cachingService.GetOrCreateAsync(cacheKey, () => lessonRepository.GetById(lessonId));
            if (lesson == null)
                throw new NotFoundException("Lesson", lessonId);

            return MapToResponse(lesson);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching lesson {LessonId}", lessonId);
            throw;
        }
    }

    public async Task<LessonResponse?> GetLessonBySlug(string slug)
    {
        if (string.IsNullOrEmpty(slug))
            throw new BadRequestException("Lesson slug cannot be null or empty");

        var cacheKey = CacheKeys.LessonBySlug(slug);

        try
        {
            var lesson = await cachingService.GetOrCreateAsync(cacheKey, () => lessonRepository.GetBySlug(slug));
            return lesson != null ? MapToResponse(lesson) : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching lesson by slug {Slug}", slug);
            throw;
        }
    }

    public async Task<List<LessonResponse>?> GetLessonsByStatus(string status)
    {
        if (string.IsNullOrEmpty(status))
            throw new BadRequestException("Status cannot be null or empty");

        try
        {
            var lessons = await lessonRepository.GetByStatus(status);
            return lessons?.Select(MapToResponse).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching lessons by status {Status}", status);
            throw;
        }
    }

    public async Task<List<LessonResponse>?> GetPublishedLessons()
    {
        try
        {
            var lessons = await lessonRepository.GetPublished();
            return lessons?.Select(MapToResponse).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching published lessons");
            throw;
        }
    }

    public async Task<List<LessonResponse>?> GetLessonsByCreator(string creatorId)
    {
        if (string.IsNullOrEmpty(creatorId))
            throw new BadRequestException("Creator ID cannot be null or empty");

        try
        {
            var lessons = await lessonRepository.GetByCreator(creatorId);
            return lessons?.Select(MapToResponse).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching lessons by creator {CreatorId}", creatorId);
            throw;
        }
    }

    public async Task<List<LessonResponse>?> GetLessonsByDifficulty(string difficulty)
    {
        if (string.IsNullOrEmpty(difficulty))
            throw new BadRequestException("Difficulty cannot be null or empty");

        try
        {
            var lessons = await lessonRepository.GetByDifficulty(difficulty);
            return lessons?.Select(MapToResponse).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching lessons by difficulty {Difficulty}", difficulty);
            throw;
        }
    }

    #endregion

    #region CRUD Operations

    public async Task<LessonResponse> CreateLesson(CreateLessonRequest request, string userId)
    {
        if (request == null)
            throw new BadRequestException("Lesson request cannot be null");

        if (string.IsNullOrEmpty(userId))
            throw new BadRequestException("User ID cannot be null or empty");

        try
        {
            // Verify course exists
            var course = await courseRepository.GetById(request.CourseId);
            if (course == null)
                throw new NotFoundException("Course", request.CourseId);

            // Check if slug already exists
            var existingLesson = await lessonRepository.GetBySlug(request.Slug);
            if (existingLesson != null)
                throw new BadRequestException($"Lesson with slug '{request.Slug}' already exists");

            // Get next order number if not specified
            if (request.Order <= 0)
            {
                request.Order = await lessonRepository.GetNextOrderNumber(request.CourseId);
            }

            var lesson = new Lesson
            {
                CourseId = request.CourseId,
                Title = request.Title,
                Slug = request.Slug,
                Content = request.Content,
                Language = request.Language,
                Template = request.Template,
                Order = request.Order,
                TestConfig = request.TestConfig,
                AdditionalResources = request.AdditionalResources,
                Tags = request.Tags,
                Status = request.Status,
                CreatedBy = Guid.Parse(userId),
                Difficulty = "beginner",
                EstimatedTimeMinutes = 30
            };

            var createdLesson = await lessonRepository.Create(lesson);

            // Invalidate cache
            await InvalidateLessonCaches(request.CourseId);

            return MapToResponse(createdLesson);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating lesson {Title}", request.Title);
            throw;
        }
    }

    public async Task<LessonResponse?> UpdateLesson(string lessonId, UpdateLessonRequest request, string userId)
    {
        if (string.IsNullOrEmpty(lessonId))
            throw new BadRequestException("Lesson ID cannot be null or empty");

        if (request == null)
            throw new BadRequestException("Update request cannot be null");

        if (string.IsNullOrEmpty(userId))
            throw new BadRequestException("User ID cannot be null or empty");

        try
        {
            var existingLesson = await lessonRepository.GetById(lessonId);
            if (existingLesson == null)
                throw new NotFoundException("Lesson", lessonId);

            // Check if new slug conflicts with existing lessons
            if (!string.IsNullOrEmpty(request.Slug) && request.Slug != existingLesson.Slug)
            {
                var conflictingLesson = await lessonRepository.GetBySlug(request.Slug);
                if (conflictingLesson != null)
                    throw new BadRequestException($"Lesson with slug '{request.Slug}' already exists");
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(request.Title))
                existingLesson.Title = request.Title;
            if (!string.IsNullOrEmpty(request.Slug))
                existingLesson.Slug = request.Slug;
            if (!string.IsNullOrEmpty(request.Content))
                existingLesson.Content = request.Content;
            if (!string.IsNullOrEmpty(request.Language))
                existingLesson.Language = request.Language;
            if (!string.IsNullOrEmpty(request.Template))
                existingLesson.Template = request.Template;
            if (request.Order.HasValue)
                existingLesson.Order = request.Order.Value;
            if (request.TestConfig != null)
                existingLesson.TestConfig = request.TestConfig;
            if (request.AdditionalResources != null)
                existingLesson.AdditionalResources = request.AdditionalResources;
            if (request.Tags != null)
                existingLesson.Tags = request.Tags;
            if (!string.IsNullOrEmpty(request.Status))
                existingLesson.Status = request.Status;

            existingLesson.UpdatedBy = Guid.Parse(userId);

            var updatedLesson = await lessonRepository.Update(lessonId, existingLesson);

            // Invalidate cache
            await InvalidateLessonCaches(existingLesson.CourseId, lessonId);

            return updatedLesson != null ? MapToResponse(updatedLesson) : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating lesson {LessonId}", lessonId);
            throw;
        }
    }

    public async Task<bool> DeleteLesson(string lessonId, string userId)
    {
        if (string.IsNullOrEmpty(lessonId))
            throw new BadRequestException("Lesson ID cannot be null or empty");

        if (string.IsNullOrEmpty(userId))
            throw new BadRequestException("User ID cannot be null or empty");

        try
        {
            var lesson = await lessonRepository.GetById(lessonId);
            if (lesson == null)
                throw new NotFoundException("Lesson", lessonId);

            // Use soft delete for published lessons, hard delete for drafts
            var result = lesson.IsPublished
                ? await lessonRepository.SoftDelete(lessonId)
                : await lessonRepository.Delete(lessonId);

            if (result)
            {
                await InvalidateLessonCaches(lesson.CourseId, lessonId);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting lesson {LessonId}", lessonId);
            throw;
        }
    }

    #endregion

    #region Ordering Operations

    public async Task<bool> ReorderLessons(string courseId, List<string> lessonIds, string userId)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        if (lessonIds == null || lessonIds.Count == 0)
            throw new BadRequestException("Lesson IDs cannot be null or empty");

        if (string.IsNullOrEmpty(userId))
            throw new BadRequestException("User ID cannot be null or empty");

        try
        {
            var result = await lessonRepository.ReorderLessons(courseId, lessonIds);

            if (result)
            {
                await InvalidateLessonCaches(courseId);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reordering lessons for course {CourseId}", courseId);
            throw;
        }
    }

    public async Task<List<LessonResponse>> GetOrderedLessonsByCourseId(string courseId)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        try
        {
            var lessons = await lessonRepository.GetOrderedByCourseId(courseId);
            return lessons?.Select(MapToResponse).ToList() ?? new List<LessonResponse>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching ordered lessons for course {CourseId}", courseId);
            throw;
        }
    }

    #endregion

    #region Publishing Operations

    public async Task<LessonResponse?> PublishLesson(string lessonId, string userId)
    {
        if (string.IsNullOrEmpty(lessonId))
            throw new BadRequestException("Lesson ID cannot be null or empty");

        if (string.IsNullOrEmpty(userId))
            throw new BadRequestException("User ID cannot be null or empty");

        try
        {
            var lesson = await lessonRepository.GetById(lessonId);
            if (lesson == null)
                throw new NotFoundException("Lesson", lessonId);

            // Validate lesson before publishing
            var validationErrors = await ValidateLessonForPublishing(lesson);
            if (validationErrors.Count > 0)
            {
                await lessonRepository.Validate(lessonId, userId, validationErrors);
                throw new BadRequestException($"Lesson validation failed: {string.Join(", ", validationErrors)}");
            }

            await lessonRepository.MarkAsValidated(lessonId, userId);
            await lessonRepository.Publish(lessonId, userId);

            await InvalidateLessonCaches(lesson.CourseId, lessonId);

            var publishedLesson = await lessonRepository.GetById(lessonId);
            return publishedLesson != null ? MapToResponse(publishedLesson) : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing lesson {LessonId}", lessonId);
            throw;
        }
    }

    public async Task<LessonResponse?> UnpublishLesson(string lessonId, string userId)
    {
        if (string.IsNullOrEmpty(lessonId))
            throw new BadRequestException("Lesson ID cannot be null or empty");

        if (string.IsNullOrEmpty(userId))
            throw new BadRequestException("User ID cannot be null or empty");

        try
        {
            var lesson = await lessonRepository.GetById(lessonId);
            if (lesson == null)
                throw new NotFoundException("Lesson", lessonId);

            await lessonRepository.Unpublish(lessonId, userId);

            await InvalidateLessonCaches(lesson.CourseId, lessonId);

            var unpublishedLesson = await lessonRepository.GetById(lessonId);
            return unpublishedLesson != null ? MapToResponse(unpublishedLesson) : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error unpublishing lesson {LessonId}", lessonId);
            throw;
        }
    }

    public async Task<LessonResponse?> PreviewLesson(string lessonId, string userId)
    {
        if (string.IsNullOrEmpty(lessonId))
            throw new BadRequestException("Lesson ID cannot be null or empty");

        try
        {
            var lesson = await lessonRepository.GetById(lessonId);
            if (lesson == null)
                throw new NotFoundException("Lesson", lessonId);

            // Track preview view
            await lessonRepository.IncrementViewCount(lessonId);
            await InvalidateLessonCaches(lesson.CourseId, lessonId);

            return MapToResponse(lesson);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error previewing lesson {LessonId}", lessonId);
            throw;
        }
    }

    #endregion

    #region Validation Operations

    public async Task<LessonResponse?> ValidateLesson(string lessonId, string userId)
    {
        if (string.IsNullOrEmpty(lessonId))
            throw new BadRequestException("Lesson ID cannot be null or empty");

        if (string.IsNullOrEmpty(userId))
            throw new BadRequestException("User ID cannot be null or empty");

        try
        {
            var lesson = await lessonRepository.GetById(lessonId);
            if (lesson == null)
                throw new NotFoundException("Lesson", lessonId);

            var validationErrors = await ValidateLessonForPublishing(lesson);

            if (validationErrors.Count == 0)
            {
                await lessonRepository.MarkAsValidated(lessonId, userId);
            }
            else
            {
                await lessonRepository.Validate(lessonId, userId, validationErrors);
            }

            await InvalidateLessonCaches(lesson.CourseId, lessonId);

            var validatedLesson = await lessonRepository.GetById(lessonId);
            return validatedLesson != null ? MapToResponse(validatedLesson) : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating lesson {LessonId}", lessonId);
            throw;
        }
    }

    public async Task<List<string>> GetLessonValidationErrors(string lessonId)
    {
        if (string.IsNullOrEmpty(lessonId))
            throw new BadRequestException("Lesson ID cannot be null or empty");

        try
        {
            var lesson = await lessonRepository.GetById(lessonId);
            if (lesson == null)
                throw new NotFoundException("Lesson", lessonId);

            return lesson.ValidationErrors;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching validation errors for lesson {LessonId}", lessonId);
            throw;
        }
    }

    #endregion

    #region Analytics Operations

    public async Task<bool> IncrementLessonViews(string lessonId)
    {
        if (string.IsNullOrEmpty(lessonId))
            throw new BadRequestException("Lesson ID cannot be null or empty");

        try
        {
            var result = await lessonRepository.IncrementViewCount(lessonId);
            if (result)
            {
                var lesson = await lessonRepository.GetById(lessonId);
                if (lesson != null)
                {
                    await InvalidateLessonCaches(lesson.CourseId, lessonId);
                }
            }
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error incrementing views for lesson {LessonId}", lessonId);
            throw;
        }
    }

    public async Task<bool> TrackLessonCompletion(string lessonId, string userId, double timeSpent)
    {
        if (string.IsNullOrEmpty(lessonId))
            throw new BadRequestException("Lesson ID cannot be null or empty");

        if (string.IsNullOrEmpty(userId))
            throw new BadRequestException("User ID cannot be null or empty");

        try
        {
            var incrementResult = await lessonRepository.IncrementCompletionCount(lessonId);
            var timeResult = await lessonRepository.UpdateAverageTimeSpent(lessonId, timeSpent);

            if (incrementResult || timeResult)
            {
                var lesson = await lessonRepository.GetById(lessonId);
                if (lesson != null)
                {
                    await InvalidateLessonCaches(lesson.CourseId, lessonId);
                }
            }

            return incrementResult && timeResult;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error tracking completion for lesson {LessonId}", lessonId);
            throw;
        }
    }

    #endregion

    #region Search and Filtering

    public async Task<List<LessonResponse>?> SearchLessons(string searchTerm, string? courseId = null, string? difficulty = null, List<string>? tags = null)
    {
        try
        {
            var lessons = await lessonRepository.Search(searchTerm, courseId, difficulty, tags);
            return lessons?.Select(MapToResponse).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching lessons with term {SearchTerm}", searchTerm);
            throw;
        }
    }

    public async Task<List<LessonResponse>?> GetPaginatedLessons(int page, int pageSize, string? courseId = null, string? sortBy = null, bool ascending = true)
    {
        if (page < 1)
            throw new BadRequestException("Page must be greater than 0");

        if (pageSize < 1 || pageSize > 100)
            throw new BadRequestException("Page size must be between 1 and 100");

        try
        {
            var lessons = await lessonRepository.GetPaginated(page, pageSize, courseId, sortBy, ascending);
            return lessons?.Select(MapToResponse).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paginated lessons");
            throw;
        }
    }

    #endregion

    #region Prerequisites

    public async Task<List<LessonResponse>?> GetLessonPrerequisites(string lessonId)
    {
        if (string.IsNullOrEmpty(lessonId))
            throw new BadRequestException("Lesson ID cannot be null or empty");

        try
        {
            var prerequisites = await lessonRepository.GetPrerequisites(lessonId);
            return prerequisites?.Select(MapToResponse).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching prerequisites for lesson {LessonId}", lessonId);
            throw;
        }
    }

    public async Task<bool> CanAccessLesson(string lessonId, string userId)
    {
        if (string.IsNullOrEmpty(lessonId))
            throw new BadRequestException("Lesson ID cannot be null or empty");

        if (string.IsNullOrEmpty(userId))
            throw new BadRequestException("User ID cannot be null or empty");

        try
        {
            return await lessonRepository.HasCompletedPrerequisites(lessonId, userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking lesson access for lesson {LessonId} and user {UserId}", lessonId, userId);
            throw;
        }
    }

    #endregion

    #region Private Helper Methods

    private static LessonResponse MapToResponse(Lesson lesson)
    {
        return new LessonResponse
        {
            Id = lesson.Id,
            CourseId = lesson.CourseId,
            Title = lesson.Title,
            Slug = lesson.Slug,
            Content = lesson.Content,
            Template = lesson.Template,
            CreatedAt = lesson.CreatedAt,
            UpdatedAt = lesson.UpdatedAt,
            Language = lesson.Language,
            Order = lesson.Order,
            AdditionalResources = lesson.AdditionalResources,
            Tags = lesson.Tags,
            TestCases = lesson.TestConfig.TestCases,
            MainFunction = lesson.TestConfig.TestTemplate ?? ""
        };
    }

    private async Task<List<string>> ValidateLessonForPublishing(Lesson lesson)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(lesson.Title))
            errors.Add("Lesson title is required");

        if (string.IsNullOrEmpty(lesson.Content) || lesson.Content.Length < 10)
            errors.Add("Lesson content must be at least 10 characters long");

        if (string.IsNullOrEmpty(lesson.Template))
            errors.Add("Lesson template is required");

        if (lesson.TestConfig == null || lesson.TestConfig.TestCases.Count == 0)
            errors.Add("Lesson must have at least one test case");

        // Add more validation rules as needed
        return errors;
    }

    private async Task InvalidateLessonCaches(string courseId, string? lessonId = null)
    {
        try
        {
            await cachingService.RemoveAsync(CacheKeys.LessonsByCourseId(courseId));

            if (!string.IsNullOrEmpty(lessonId))
            {
                await cachingService.RemoveAsync(CacheKeys.LessonById(lessonId));
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error invalidating lesson caches");
        }
    }

    #endregion
}