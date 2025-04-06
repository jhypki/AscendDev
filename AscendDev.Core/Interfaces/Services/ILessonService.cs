using AscendDev.Core.DTOs.Courses;

namespace AscendDev.Core.Interfaces.Services;

public interface ILessonService
{
    Task<List<LessonResponse>> GetLessonsByCourseId(string courseId);
    Task<LessonResponse> GetLessonById(string lessonId);
}