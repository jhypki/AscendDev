using AscendDev.Core.Models.Courses;

namespace AscendDev.Core.Interfaces.Data;

public interface ICourseRepository
{
    Task<List<Course>> GetAll();
    Task<Course> GetById(string courseId);
    Task<Course> GetBySlug(string slug);
    Task<Course> GetByLanguage(string language);
    Task<List<Course>> GetByTag(string tag);
}