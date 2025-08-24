using AscendDev.Core.Models.Courses;

namespace AscendDev.Core.Interfaces.Data;

public interface ICourseRepository
{
    // Read operations
    Task<List<Course>?> GetAll();
    Task<Course?> GetById(string courseId);
    Task<Course?> GetBySlug(string slug);
    Task<Course?> GetByLanguage(string language);
    Task<List<Course>?> GetByTag(string tag);
    Task<List<Course>?> GetByStatus(string status);
    Task<List<Course>?> GetPublishedCourses();

    // Write operations
    Task<Course> Create(Course course);
    Task<Course> Update(Course course);
    Task<bool> Delete(string courseId);
    Task<bool> UpdateStatus(string courseId, string status);

    // Analytics operations
    Task<int> GetTotalCount();
    Task<int> GetCountByStatus(string status);
    Task<Dictionary<string, int>> GetCourseStatistics();
}