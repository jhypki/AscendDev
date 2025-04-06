using AscendDev.Core.Models.Courses;

namespace AscendDev.Core.Interfaces.Services;

public interface ICourseService
{
    Task<List<Course>> GetAllCourses();
    Task<Course> GetCourseById(string courseId);

    Task<Course> GetCourseBySlug(string slug);
    // Task<List<Course>> GetCoursesByTags(List<string> tags);
    // Task<List<Course>> GetCoursesByLanguage(string language);
}