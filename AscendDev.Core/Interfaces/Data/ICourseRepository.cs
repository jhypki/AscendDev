using AscendDev.Core.Models.Courses;
using MongoDB.Bson;

namespace AscendDev.Core.Interfaces.Data;

public interface ICourseRepository
{
    Task<List<Course>> GetAll();
    Task<Course> GetById(ObjectId courseId);
    Task<Course> GetBySlug(string slug);
}