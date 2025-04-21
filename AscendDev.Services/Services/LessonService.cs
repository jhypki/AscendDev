using AscendDev.Core.Caching;
using AscendDev.Core.DTOs.Courses;
using AscendDev.Core.Exceptions;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AscendDev.Services.Services;

public class LessonService(
    ILessonRepository lessonRepository,
    ILogger<ILessonService> logger,
    ICachingService cachingService) : ILessonService
{
    public async Task<List<LessonResponse>> GetLessonsByCourseId(string courseId)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        var cacheKey = CacheKeys.LessonsByCourseId(courseId);

        var lessons = await cachingService.GetOrCreateAsync(cacheKey, () => lessonRepository.GetByCourseId(courseId));
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
            TestCases = lesson.TestConfig.TestCases
        }).ToList();
    }

    public async Task<LessonResponse> GetLessonById(string lessonId)
    {
        if (string.IsNullOrEmpty(lessonId))
            throw new BadRequestException("Lesson ID cannot be null or empty");

        var cacheKey = CacheKeys.LessonById(lessonId);

        var lesson = await cachingService.GetOrCreateAsync(cacheKey, () => lessonRepository.GetById(lessonId));
        var jsonString = JsonConvert.SerializeObject(lesson, Formatting.Indented);
        Console.WriteLine($"Lesson object: {jsonString}");
        if (lesson == null)
            throw new NotFoundException("Lesson", lessonId.Replace('\n', '_').Replace('\r', '_'));

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
            TestCases = lesson.TestConfig.TestCases
        };
    }
}