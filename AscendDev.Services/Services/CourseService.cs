using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.Courses;
using Microsoft.Extensions.Logging;

namespace AscendDev.Services.Services;

public class CourseService(ICourseRepository courseRepository, ILogger<CourseService> logger)
    : ICourseService
{
    public async Task<List<Course>> GetAllCourses()
    {
        try
        {
            return await courseRepository.GetAll();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching all courses");
            throw;
        }
    }

    public async Task<Course> GetCourseById(string courseId)
    {
        try
        {
            return await courseRepository.GetById(courseId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error fetching course with ID {courseId}");
            throw;
        }
    }

    public async Task<Course> GetCourseBySlug(string slug)
    {
        try
        {
            return await courseRepository.GetBySlug(slug);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error fetching course with slug {slug}");
            throw;
        }
    }
}