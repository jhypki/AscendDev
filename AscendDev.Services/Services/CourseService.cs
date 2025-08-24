using AscendDev.Core.Caching;
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
}