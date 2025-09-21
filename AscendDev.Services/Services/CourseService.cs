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
    public async Task<List<Course>?> GetAllCourses()
    {
        var cacheKey = CacheKeys.CourseAll();
        try
        {
            return await cachingService.GetOrCreateAsync(cacheKey, courseRepository.GetAll);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching all courses");
            throw;
        }
    }

    public async Task<Course?> GetCourseById(string courseId)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        var cacheKey = CacheKeys.CourseById(courseId);
        try
        {
            return await cachingService.GetOrCreateAsync(cacheKey, () => courseRepository.GetById(courseId));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error fetching course with ID {courseId.Replace('\n', '_').Replace('\r', '_')}");
            throw;
        }
    }

    public async Task<Course?> GetCourseBySlug(string slug)
    {
        if (string.IsNullOrEmpty(slug))
            throw new BadRequestException("Course slug cannot be null or empty");

        var cacheKey = CacheKeys.CourseBySlug(slug);
        try
        {
            return await cachingService.GetOrCreateAsync(cacheKey, () => courseRepository.GetBySlug(slug));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error fetching course with slug {slug.Replace('\n', '_').Replace('\r', '_')}");
            throw;
        }
    }

    public async Task<List<Course>?> GetCoursesByTag(string tag)
    {
        if (string.IsNullOrEmpty(tag))
            throw new BadRequestException("Tag cannot be null or empty");

        var cacheKey = CacheKeys.CourseByTag(tag);
        try
        {
            return await cachingService.GetOrCreateAsync(cacheKey, () => courseRepository.GetByTag(tag));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error fetching courses with tag {tag.Replace('\n', '_').Replace('\r', '_')}");
            throw;
        }
    }

    public async Task<List<Course>?> GetCoursesByLanguage(string language)
    {
        if (string.IsNullOrEmpty(language))
            throw new BadRequestException("Language cannot be null or empty");

        try
        {
            var course = await courseRepository.GetByLanguage(language);
            return course != null
                ? new List<Course> { course }
                : new List<Course>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error fetching courses with language {language.Replace('\n', '_').Replace('\r', '_')}");
            throw;
        }
    }

    public async Task<List<Course>?> GetCoursesByStatus(string status)
    {
        if (string.IsNullOrEmpty(status))
            throw new BadRequestException("Status cannot be null or empty");

        var cacheKey = CacheKeys.CourseByStatus(status);
        try
        {
            return await cachingService.GetOrCreateAsync(cacheKey, () => courseRepository.GetByStatus(status));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error fetching courses with status {status.Replace('\n', '_').Replace('\r', '_')}");
            throw;
        }
    }

    public async Task<List<Course>?> GetPublishedCourses()
    {
        var cacheKey = CacheKeys.PublishedCourses();
        try
        {
            return await cachingService.GetOrCreateAsync(cacheKey, courseRepository.GetPublishedCourses);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching published courses");
            throw;
        }
    }

    public async Task<PaginatedCoursesResponse> GetCoursesAsync(CourseQueryRequest request)
    {
        try
        {
            // Get all courses first (we'll implement repository-level filtering later)
            var allCourses = await courseRepository.GetAll();
            if (allCourses == null)
                allCourses = new List<Course>();

            // Apply filtering
            var filteredCourses = allCourses.AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(request.Search))
            {
                var searchTerm = request.Search.ToLower();
                filteredCourses = filteredCourses.Where(c =>
                    c.Title.ToLower().Contains(searchTerm) ||
                    (c.Description != null && c.Description.ToLower().Contains(searchTerm)) ||
                    c.Language.ToLower().Contains(searchTerm) ||
                    (c.Tags != null && c.Tags.Any(tag => tag.ToLower().Contains(searchTerm))));
            }

            // Language filter
            if (!string.IsNullOrEmpty(request.Language))
            {
                filteredCourses = filteredCourses.Where(c =>
                    c.Language.Equals(request.Language, StringComparison.OrdinalIgnoreCase));
            }

            // Status filter
            if (!string.IsNullOrEmpty(request.Status))
            {
                filteredCourses = filteredCourses.Where(c =>
                    c.Status.Equals(request.Status, StringComparison.OrdinalIgnoreCase));
            }

            // Tags filter
            if (request.Tags != null && request.Tags.Any())
            {
                filteredCourses = filteredCourses.Where(c =>
                    c.Tags != null && request.Tags.Any(tag =>
                        c.Tags.Any(courseTag => courseTag.Equals(tag, StringComparison.OrdinalIgnoreCase))));
            }

            // Apply sorting
            filteredCourses = request.SortBy?.ToLower() switch
            {
                "title" => request.SortOrder?.ToLower() == "desc"
                    ? filteredCourses.OrderByDescending(c => c.Title)
                    : filteredCourses.OrderBy(c => c.Title),
                "language" => request.SortOrder?.ToLower() == "desc"
                    ? filteredCourses.OrderByDescending(c => c.Language)
                    : filteredCourses.OrderBy(c => c.Language),
                "status" => request.SortOrder?.ToLower() == "desc"
                    ? filteredCourses.OrderByDescending(c => c.Status)
                    : filteredCourses.OrderBy(c => c.Status),
                "updatedat" => request.SortOrder?.ToLower() == "desc"
                    ? filteredCourses.OrderByDescending(c => c.UpdatedAt)
                    : filteredCourses.OrderBy(c => c.UpdatedAt),
                _ => request.SortOrder?.ToLower() == "desc"
                    ? filteredCourses.OrderByDescending(c => c.CreatedAt)
                    : filteredCourses.OrderBy(c => c.CreatedAt)
            };

            var totalCount = filteredCourses.Count();

            // Apply pagination
            var paginatedCourses = filteredCourses
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PaginatedCoursesResponse(paginatedCourses, totalCount, request.Page, request.PageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paginated courses");
            throw;
        }
    }

    public async Task<Course> CreateCourse(Course course)
    {
        if (course == null)
            throw new BadRequestException("Course cannot be null");

        var validationErrors = await GetCourseValidationErrors(course);
        if (validationErrors.Count > 0)
            throw new BadRequestException($"Course validation failed: {string.Join(", ", validationErrors)}");

        try
        {
            var createdCourse = await courseRepository.Create(course);

            // Invalidate relevant caches
            await cachingService.RemoveAsync(CacheKeys.CourseAll());
            await cachingService.RemoveAsync(CacheKeys.CourseByStatus(course.Status));

            return createdCourse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating course {Title}", course.Title);
            throw;
        }
    }

    public async Task<Course> UpdateCourse(Course course)
    {
        if (course == null)
            throw new BadRequestException("Course cannot be null");

        if (string.IsNullOrEmpty(course.Id))
            throw new BadRequestException("Course ID cannot be null or empty");

        var validationErrors = await GetCourseValidationErrors(course);
        if (validationErrors.Count > 0)
            throw new BadRequestException($"Course validation failed: {string.Join(", ", validationErrors)}");

        try
        {
            var updatedCourse = await courseRepository.Update(course);

            // Invalidate relevant caches
            await cachingService.RemoveAsync(CacheKeys.CourseById(course.Id));
            await cachingService.RemoveAsync(CacheKeys.CourseBySlug(course.Slug));
            await cachingService.RemoveAsync(CacheKeys.CourseAll());
            await cachingService.RemoveAsync(CacheKeys.CourseByStatus(course.Status));

            return updatedCourse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating course {Id}", course.Id);
            throw;
        }
    }

    public async Task<bool> DeleteCourse(string courseId)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        try
        {
            var course = await courseRepository.GetById(courseId);
            if (course == null)
                throw new NotFoundException("Course", courseId);

            var result = await courseRepository.Delete(courseId);

            if (result)
            {
                // Invalidate relevant caches
                await cachingService.RemoveAsync(CacheKeys.CourseById(courseId));
                await cachingService.RemoveAsync(CacheKeys.CourseBySlug(course.Slug));
                await cachingService.RemoveAsync(CacheKeys.CourseAll());
                await cachingService.RemoveAsync(CacheKeys.CourseByStatus(course.Status));
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting course {CourseId}", courseId);
            throw;
        }
    }

    public async Task<bool> PublishCourse(string courseId)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        try
        {
            var course = await courseRepository.GetById(courseId);
            if (course == null)
                throw new NotFoundException("Course", courseId);

            var result = await courseRepository.UpdateStatus(courseId, "published");

            if (result)
            {
                // Invalidate relevant caches
                await cachingService.RemoveAsync(CacheKeys.CourseById(courseId));
                await cachingService.RemoveAsync(CacheKeys.CourseBySlug(course.Slug));
                await cachingService.RemoveAsync(CacheKeys.CourseAll());
                await cachingService.RemoveAsync(CacheKeys.CourseByStatus("draft"));
                await cachingService.RemoveAsync(CacheKeys.CourseByStatus("published"));
                await cachingService.RemoveAsync(CacheKeys.PublishedCourses());
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing course {CourseId}", courseId);
            throw;
        }
    }

    public async Task<bool> UnpublishCourse(string courseId)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        try
        {
            var course = await courseRepository.GetById(courseId);
            if (course == null)
                throw new NotFoundException("Course", courseId);

            var result = await courseRepository.UpdateStatus(courseId, "draft");

            if (result)
            {
                // Invalidate relevant caches
                await cachingService.RemoveAsync(CacheKeys.CourseById(courseId));
                await cachingService.RemoveAsync(CacheKeys.CourseBySlug(course.Slug));
                await cachingService.RemoveAsync(CacheKeys.CourseAll());
                await cachingService.RemoveAsync(CacheKeys.CourseByStatus("draft"));
                await cachingService.RemoveAsync(CacheKeys.CourseByStatus("published"));
                await cachingService.RemoveAsync(CacheKeys.PublishedCourses());
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error unpublishing course {CourseId}", courseId);
            throw;
        }
    }

    public async Task<Course?> PreviewCourse(string courseId)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        try
        {
            // Don't use cache for preview to always get fresh data
            return await courseRepository.GetById(courseId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error previewing course {CourseId}", courseId);
            throw;
        }
    }

    public async Task<int> GetTotalCourseCount()
    {
        try
        {
            return await courseRepository.GetTotalCount();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting total course count");
            throw;
        }
    }

    public async Task<int> GetCourseCountByStatus(string status)
    {
        if (string.IsNullOrEmpty(status))
            throw new BadRequestException("Status cannot be null or empty");

        try
        {
            return await courseRepository.GetCountByStatus(status);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting course count by status {Status}", status);
            throw;
        }
    }

    public async Task<Dictionary<string, int>> GetCourseStatistics()
    {
        try
        {
            return await courseRepository.GetCourseStatistics();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting course statistics");
            throw;
        }
    }

    public async Task<bool> ValidateCourse(Course course)
    {
        var errors = await GetCourseValidationErrors(course);
        return errors.Count == 0;
    }

    public async Task<List<string>> GetCourseValidationErrors(Course course)
    {
        var errors = new List<string>();

        if (course == null)
        {
            errors.Add("Course cannot be null");
            return errors;
        }

        // Basic validation
        if (string.IsNullOrWhiteSpace(course.Title))
            errors.Add("Title is required");

        if (string.IsNullOrWhiteSpace(course.Slug))
            errors.Add("Slug is required");

        if (string.IsNullOrWhiteSpace(course.Description))
            errors.Add("Description is required");

        if (string.IsNullOrWhiteSpace(course.Language))
            errors.Add("Language is required");

        // Check for duplicate slug
        if (!string.IsNullOrWhiteSpace(course.Slug))
        {
            try
            {
                var existingCourse = await courseRepository.GetBySlug(course.Slug);
                if (existingCourse != null && existingCourse.Id != course.Id)
                    errors.Add("Slug must be unique");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking for duplicate slug");
                errors.Add("Error validating slug uniqueness");
            }
        }

        return errors;
    }
}