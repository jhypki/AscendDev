using AscendDev.Core.Models.Courses;

namespace AscendDev.Core.Interfaces.Data;

public interface ILessonRepository
{
    Task<List<Lesson>?> GetByCourseId(string courseId);
    Task<Lesson?> GetById(string lessonId);
    Task<Lesson?> GetBySlug(string slug);
}