using AscendDev.Core.Caching;
using AscendDev.Core.DTOs.Courses;
using AscendDev.Core.Exceptions;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.Courses;
using Microsoft.Extensions.Logging;

namespace AscendDev.Services.Services;

public class CourseService(
    ICourseRepository courseRepository,
    ILogger<CourseService> logger,
    ICachingService cachingService)
    : ICourseService
{
    #region Read Operations

    public async Task<List<CourseResponse>?> GetAllCourses()
    {
        var cacheKey = CacheKeys.CourseAll();
        try
        {
            var courses = await cachingService.GetOrCreateAsync(cacheKey, courseRepository.GetAll);
            return courses?.Select(MapToResponse).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching all courses");
            throw;
        }
    }

    public async Task<CourseResponse?> GetCourseById(string courseId)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        var cacheKey = CacheKeys.CourseById(courseId);
        try
        {
            var course = await cachingService.GetOrCreateAsync(cacheKey, () => courseRepository.GetById(courseId));
            return course != null ? MapToResponse(course) : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching course with ID {CourseId}", courseId);
            throw;
        }
    }

    public async Task<CourseResponse?> GetCourseBySlug(string slug)
    {
        if (string.IsNullOrEmpty(slug))
            throw new BadRequestException("Course slug cannot be null or empty");

        var cacheKey = CacheKeys.CourseBySlug(slug);
        try
        {
            var course = await cachingService.GetOrCreateAsync(cacheKey, () => courseRepository.GetBySlug(slug));
            return course != null ? MapToResponse(course) : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching course with slug {Slug}", slug);
            throw;
        }
    }

    public async Task<List<CourseResponse>?> GetCoursesByLanguage(string language)
    {
        if (string.IsNullOrEmpty(language))
            throw new BadRequestException("Language cannot be null or empty");

        try
        {
            var courses = await courseRepository.Search("", language);
            return courses?.Select(MapToResponse).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching courses by language {Language}", language);
            throw;
        }
    }

    public async Task<List<CourseResponse>?> GetCoursesByTags(List<string> tags)
    {
        if (tags == null || tags.Count == 0)
            throw new BadRequestException("Tags cannot be null or empty");

        try
        {
            var courses = await courseRepository.Search("", null, tags);
            return courses?.Select(MapToResponse).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching courses by tags");
            throw;
        }
    }

    public async Task<List<CourseResponse>?> GetCoursesByStatus(string status)
    {
        if (string.IsNullOrEmpty(status))
            throw new BadRequestException("Status cannot be null or empty");

        try
        {
            var courses = await courseRepository.GetByStatus(status);
            return courses?.Select(MapToResponse).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching courses by status {Status}", status);
            throw;
        }
    }

    public async Task<List<CourseResponse>?> GetPublishedCourses()
    {
        var cacheKey = CacheKeys.CoursePublished();
        try
        {
            var courses = await cachingService.GetOrCreateAsync(cacheKey, courseRepository.GetPublished);
            return courses?.Select(MapToResponse).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching published courses");
            throw;
        }
    }

    public async Task<List<CourseResponse>?> GetCoursesByCreator(string creatorId)
    {
        if (string.IsNullOrEmpty(creatorId))
            throw new BadRequestException("Creator ID cannot be null or empty");

        try
        {
            var courses = await courseRepository.GetByCreator(creatorId);
            return courses?.Select(MapToResponse).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching courses by creator {CreatorId}", creatorId);
            throw;
        }
    }

    #endregion

    #region CRUD Operations

    public async Task<CourseResponse> CreateCourse(CreateCourseRequest request, string userId)
    {
        if (request == null)
            throw new BadRequestException("Course request cannot be null");

        if (string.IsNullOrEmpty(userId))
            throw new BadRequestException("User ID cannot be null or empty");

        try
        {
            // Check if slug already exists
            var existingCourse = await courseRepository.GetBySlug(request.Slug);
            if (existingCourse != null)
                throw new BadRequestException($"Course with slug '{request.Slug}' already exists");

            var course = new Course
            {
                Title = request.Title,
                Slug = request.Slug,
                Description = request.Description,
                Language = request.Language,
                FeaturedImage = request.FeaturedImage,
                Tags = request.Tags,
                Status = request.Status,
                CreatedBy = Guid.Parse(userId),
                Version = 1
            };

            var createdCourse = await courseRepository.Create(course);

            // Invalidate cache
            await InvalidateCourseCaches();

            return MapToResponse(createdCourse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating course {Title}", request.Title);
            throw;
        }
    }

    public async Task<CourseResponse?> UpdateCourse(string courseId, UpdateCourseRequest request, string userId)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        if (request == null)
            throw new BadRequestException("Update request cannot be null");

        if (string.IsNullOrEmpty(userId))
            throw new BadRequestException("User ID cannot be null or empty");

        try
        {
            var existingCourse = await courseRepository.GetById(courseId);
            if (existingCourse == null)
                throw new NotFoundException("Course", courseId);

            // Check if new slug conflicts with existing courses
            if (!string.IsNullOrEmpty(request.Slug) && request.Slug != existingCourse.Slug)
            {
                var conflictingCourse = await courseRepository.GetBySlug(request.Slug);
                if (conflictingCourse != null)
                    throw new BadRequestException($"Course with slug '{request.Slug}' already exists");
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(request.Title))
                existingCourse.Title = request.Title;
            if (!string.IsNullOrEmpty(request.Slug))
                existingCourse.Slug = request.Slug;
            if (!string.IsNullOrEmpty(request.Description))
                existingCourse.Description = request.Description;
            if (!string.IsNullOrEmpty(request.Language))
                existingCourse.Language = request.Language;
            if (request.FeaturedImage != null)
                existingCourse.FeaturedImage = request.FeaturedImage;
            if (request.Tags != null)
                existingCourse.Tags = request.Tags;
            if (!string.IsNullOrEmpty(request.Status))
                existingCourse.Status = request.Status;

            existingCourse.UpdatedBy = Guid.Parse(userId);

            var updatedCourse = await courseRepository.Update(courseId, existingCourse);

            // Invalidate cache
            await InvalidateCourseCaches(courseId);

            return updatedCourse != null ? MapToResponse(updatedCourse) : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating course {CourseId}", courseId);
            throw;
        }
    }

    public async Task<bool> DeleteCourse(string courseId, string userId)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        if (string.IsNullOrEmpty(userId))
            throw new BadRequestException("User ID cannot be null or empty");

        try
        {
            var course = await courseRepository.GetById(courseId);
            if (course == null)
                throw new NotFoundException("Course", courseId);

            // Use soft delete for published courses, hard delete for drafts
            var result = course.IsPublished
                ? await courseRepository.SoftDelete(courseId)
                : await courseRepository.Delete(courseId);

            if (result)
            {
                await InvalidateCourseCaches(courseId);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting course {CourseId}", courseId);
            throw;
        }
    }

    #endregion

    #region Versioning Operations

    public async Task<CourseResponse> CreateCourseVersion(string courseId, string userId)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        if (string.IsNullOrEmpty(userId))
            throw new BadRequestException("User ID cannot be null or empty");

        try
        {
            var newVersion = await courseRepository.CreateVersion(courseId, userId);
            await InvalidateCourseCaches();
            return MapToResponse(newVersion);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating version for course {CourseId}", courseId);
            throw;
        }
    }

    public async Task<List<CourseResponse>?> GetCourseVersions(string parentCourseId)
    {
        if (string.IsNullOrEmpty(parentCourseId))
            throw new BadRequestException("Parent course ID cannot be null or empty");

        try
        {
            var versions = await courseRepository.GetVersions(parentCourseId);
            return versions?.Select(MapToResponse).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching versions for course {ParentCourseId}", parentCourseId);
            throw;
        }
    }

    public async Task<CourseResponse?> GetLatestCourseVersion(string parentCourseId)
    {
        if (string.IsNullOrEmpty(parentCourseId))
            throw new BadRequestException("Parent course ID cannot be null or empty");

        try
        {
            var latestVersion = await courseRepository.GetLatestVersion(parentCourseId);
            return latestVersion != null ? MapToResponse(latestVersion) : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching latest version for course {ParentCourseId}", parentCourseId);
            throw;
        }
    }

    #endregion

    #region Publishing Workflow

    public async Task<CourseResponse?> PublishCourse(string courseId, string userId)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        if (string.IsNullOrEmpty(userId))
            throw new BadRequestException("User ID cannot be null or empty");

        try
        {
            var course = await courseRepository.GetById(courseId);
            if (course == null)
                throw new NotFoundException("Course", courseId);

            // Validate course before publishing
            var validationErrors = await ValidateCourseForPublishing(course);
            if (validationErrors.Count > 0)
            {
                await courseRepository.Validate(courseId, userId, validationErrors);
                throw new BadRequestException($"Course validation failed: {string.Join(", ", validationErrors)}");
            }

            await courseRepository.MarkAsValidated(courseId, userId);
            await courseRepository.Publish(courseId, userId);

            await InvalidateCourseCaches(courseId);

            var publishedCourse = await courseRepository.GetById(courseId);
            return publishedCourse != null ? MapToResponse(publishedCourse) : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing course {CourseId}", courseId);
            throw;
        }
    }

    public async Task<CourseResponse?> UnpublishCourse(string courseId, string userId)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        if (string.IsNullOrEmpty(userId))
            throw new BadRequestException("User ID cannot be null or empty");

        try
        {
            var course = await courseRepository.GetById(courseId);
            if (course == null)
                throw new NotFoundException("Course", courseId);

            await courseRepository.Unpublish(courseId, userId);

            await InvalidateCourseCaches(courseId);

            var unpublishedCourse = await courseRepository.GetById(courseId);
            return unpublishedCourse != null ? MapToResponse(unpublishedCourse) : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error unpublishing course {CourseId}", courseId);
            throw;
        }
    }

    public async Task<CourseResponse?> PreviewCourse(string courseId, string userId)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        try
        {
            var course = await courseRepository.GetById(courseId);
            if (course == null)
                throw new NotFoundException("Course", courseId);

            // Track preview view
            await courseRepository.IncrementViewCount(courseId);
            await InvalidateCourseCaches(courseId);

            return MapToResponse(course);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error previewing course {CourseId}", courseId);
            throw;
        }
    }

    #endregion

    #region Validation Operations

    public async Task<CourseResponse?> ValidateCourse(string courseId, string userId)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        if (string.IsNullOrEmpty(userId))
            throw new BadRequestException("User ID cannot be null or empty");

        try
        {
            var course = await courseRepository.GetById(courseId);
            if (course == null)
                throw new NotFoundException("Course", courseId);

            var validationErrors = await ValidateCourseForPublishing(course);

            if (validationErrors.Count == 0)
            {
                await courseRepository.MarkAsValidated(courseId, userId);
            }
            else
            {
                await courseRepository.Validate(courseId, userId, validationErrors);
            }

            await InvalidateCourseCaches(courseId);

            var validatedCourse = await courseRepository.GetById(courseId);
            return validatedCourse != null ? MapToResponse(validatedCourse) : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating course {CourseId}", courseId);
            throw;
        }
    }

    public async Task<List<string>> GetCourseValidationErrors(string courseId)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        try
        {
            var course = await courseRepository.GetById(courseId);
            if (course == null)
                throw new NotFoundException("Course", courseId);

            return course.ValidationErrors;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching validation errors for course {CourseId}", courseId);
            throw;
        }
    }

    #endregion

    #region Analytics Operations

    public async Task<CourseAnalyticsResponse?> GetCourseAnalytics(string courseId)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        try
        {
            return await courseRepository.GetAnalytics(courseId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching analytics for course {CourseId}", courseId);
            throw;
        }
    }

    public async Task<bool> IncrementCourseViews(string courseId)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        try
        {
            var result = await courseRepository.IncrementViewCount(courseId);
            if (result)
            {
                await InvalidateCourseCaches(courseId);
            }
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error incrementing views for course {CourseId}", courseId);
            throw;
        }
    }

    public async Task<bool> TrackCourseEnrollment(string courseId)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        try
        {
            var result = await courseRepository.IncrementEnrollmentCount(courseId);
            if (result)
            {
                await InvalidateCourseCaches(courseId);
            }
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error tracking enrollment for course {CourseId}", courseId);
            throw;
        }
    }

    #endregion

    #region Search and Filtering

    public async Task<List<CourseResponse>?> SearchCourses(string searchTerm, string? language = null, List<string>? tags = null, string? status = null)
    {
        try
        {
            var courses = await courseRepository.Search(searchTerm, language, tags, status);
            return courses?.Select(MapToResponse).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching courses with term {SearchTerm}", searchTerm);
            throw;
        }
    }

    public async Task<List<CourseResponse>?> GetPaginatedCourses(int page, int pageSize, string? sortBy = null, bool ascending = true)
    {
        if (page < 1)
            throw new BadRequestException("Page must be greater than 0");

        if (pageSize < 1 || pageSize > 100)
            throw new BadRequestException("Page size must be between 1 and 100");

        try
        {
            var courses = await courseRepository.GetPaginated(page, pageSize, sortBy, ascending);
            return courses?.Select(MapToResponse).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paginated courses");
            throw;
        }
    }

    #endregion

    #region Private Helper Methods

    private static CourseResponse MapToResponse(Course course)
    {
        return new CourseResponse
        {
            Id = course.Id,
            Title = course.Title,
            Slug = course.Slug,
            Description = course.Description,
            CreatedAt = course.CreatedAt,
            Language = course.Language,
            UpdatedAt = course.UpdatedAt,
            FeaturedImage = course.FeaturedImage,
            Tags = course.Tags,
            LessonSummaries = course.LessonSummaries,
            Status = course.Status,
            Version = course.Version,
            CreatedBy = course.CreatedBy?.ToString(),
            UpdatedBy = course.UpdatedBy?.ToString()
        };
    }

    private async Task<List<string>> ValidateCourseForPublishing(Course course)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(course.Title))
            errors.Add("Course title is required");

        if (string.IsNullOrEmpty(course.Description) || course.Description.Length < 10)
            errors.Add("Course description must be at least 10 characters long");

        if (course.LessonSummaries.Count == 0)
            errors.Add("Course must have at least one lesson");

        // Add more validation rules as needed
        return errors;
    }

    private async Task InvalidateCourseCaches(string? courseId = null)
    {
        try
        {
            await cachingService.RemoveAsync(CacheKeys.CourseAll());
            await cachingService.RemoveAsync(CacheKeys.CoursePublished());

            if (!string.IsNullOrEmpty(courseId))
            {
                await cachingService.RemoveAsync(CacheKeys.CourseById(courseId));
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error invalidating course caches");
        }
    }

    #endregion
}