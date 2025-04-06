using AscendDev.Core.DTOs.Courses;
using AscendDev.Core.Exceptions;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Services;

namespace AscendDev.Services.Services;

public class LessonService(ILessonRepository lessonRepository) : ILessonService
{
    public async Task<List<LessonResponse>> GetLessonsByCourseId(string courseId)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        var lessons = await lessonRepository.GetByCourseId(courseId);
        if (lessons == null || lessons.Count == 0)
            throw new NotFoundException("Lessons", courseId);

        return lessons.Select(lesson => new LessonResponse
        {
            Id = lesson.Id,
            CourseId = lesson.CourseId,
            Title = lesson.Title,
            Slug = lesson.Slug,
            Content = lesson.Content,
            Template = lesson.Template,
            CreatedAt = lesson.CreatedAt,
            UpdatedAt = lesson.UpdatedAt,
            Language = lesson.Language,
            Order = lesson.Order,
            AdditionalResources = lesson.AdditionalResources,
            Tags = lesson.Tags,
            MainFunction = lesson.TestConfig.MainFunction,
            TestCases = lesson.TestConfig.TestCases
        }).ToList();
    }

    public async Task<LessonResponse> GetLessonById(string lessonId)
    {
        if (string.IsNullOrEmpty(lessonId))
            throw new BadRequestException("Lesson ID cannot be null or empty");

        var lesson = await lessonRepository.GetById(lessonId);
        if (lesson == null)
            throw new NotFoundException("Lesson", lessonId);

        return new LessonResponse
        {
            Id = lesson.Id,
            CourseId = lesson.CourseId,
            Title = lesson.Title,
            Slug = lesson.Slug,
            Content = lesson.Content,
            Template = lesson.Template,
            CreatedAt = lesson.CreatedAt,
            UpdatedAt = lesson.UpdatedAt,
            Language = lesson.Language,
            Order = lesson.Order,
            AdditionalResources = lesson.AdditionalResources,
            Tags = lesson.Tags,
            MainFunction = lesson.TestConfig.MainFunction,
            TestCases = lesson.TestConfig.TestCases
        };
    }
}