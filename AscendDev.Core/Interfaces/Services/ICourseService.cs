using AscendDev.Core.Models.Courses;
using AscendDev.Core.DTOs.Courses;

namespace AscendDev.Core.Interfaces.Services;

public interface ICourseService
{
    // Read operations
    Task<List<Course>?> GetAllCourses();
    Task<PaginatedCoursesResponse> GetCoursesAsync(CourseQueryRequest request);
    Task<Course?> GetCourseById(string courseId);
    Task<Course?> GetCourseBySlug(string slug);
    Task<List<Course>?> GetCoursesByTag(string tag);
    Task<List<Course>?> GetCoursesByLanguage(string language);
    Task<List<Course>?> GetCoursesByStatus(string status);
    Task<List<Course>?> GetPublishedCourses();

    // Write operations
    Task<Course> CreateCourse(Course course);
    Task<Course> UpdateCourse(Course course);
    Task<bool> DeleteCourse(string courseId);
    Task<bool> PublishCourse(string courseId);
    Task<bool> UnpublishCourse(string courseId);
    Task<Course?> PreviewCourse(string courseId);

    // Analytics operations
    Task<int> GetTotalCourseCount();
    Task<int> GetCourseCountByStatus(string status);
    Task<Dictionary<string, int>> GetCourseStatistics();

    // Validation operations
    Task<bool> ValidateCourse(Course course);
    Task<List<string>> GetCourseValidationErrors(Course course);
}