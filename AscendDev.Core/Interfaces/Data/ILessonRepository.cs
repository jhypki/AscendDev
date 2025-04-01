using AscendDev.Core.Models.Courses;
using MongoDB.Bson;

namespace AscendDev.Core.Interfaces.Data;

public interface ILessonRepository
{
    Task<List<Lesson>> GetByCourseId(ObjectId courseId);
    Task<Lesson> GetById(ObjectId lessonId);
    Task<Lesson> GetBySlug(string slug);
}