using AscendDev.Core.Caching;
using AscendDev.Core.DTOs.Courses;
using AscendDev.Core.Exceptions;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.Courses;
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

    public async Task<List<LessonResponse>> GetLessonsByCourseIdOrdered(string courseId)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        var cacheKey = $"{CacheKeys.LessonsByCourseId(courseId)}_ordered";

        var lessons = await cachingService.GetOrCreateAsync(cacheKey, () => lessonRepository.GetByCourseIdOrdered(courseId));
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

    public async Task<LessonResponse?> GetNextLesson(string courseId, int currentOrder)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        try
        {
            var lesson = await lessonRepository.GetNextLesson(courseId, currentOrder);
            if (lesson == null)
                return null;

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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting next lesson for course {CourseId} after order {CurrentOrder}", courseId, currentOrder);
            throw;
        }
    }

    public async Task<LessonResponse?> GetPreviousLesson(string courseId, int currentOrder)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        try
        {
            var lesson = await lessonRepository.GetPreviousLesson(courseId, currentOrder);
            if (lesson == null)
                return null;

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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting previous lesson for course {CourseId} before order {CurrentOrder}", courseId, currentOrder);
            throw;
        }
    }

    public async Task<List<LessonResponse>> GetLessonsByStatus(string status)
    {
        if (string.IsNullOrEmpty(status))
            throw new BadRequestException("Status cannot be null or empty");

        try
        {
            var lessons = await lessonRepository.GetByStatus(status);
            if (lessons == null || lessons.Count == 0)
                return new List<LessonResponse>();

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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting lessons by status {Status}", status);
            throw;
        }
    }

    public async Task<LessonResponse> CreateLesson(Lesson lesson)
    {
        if (lesson == null)
            throw new BadRequestException("Lesson cannot be null");

        var validationErrors = await GetLessonValidationErrors(lesson);
        if (validationErrors.Count > 0)
            throw new BadRequestException($"Lesson validation failed: {string.Join(", ", validationErrors)}");

        try
        {
            var createdLesson = await lessonRepository.Create(lesson);

            // Invalidate relevant caches
            await cachingService.RemoveAsync(CacheKeys.LessonsByCourseId(lesson.CourseId));
            await cachingService.RemoveAsync($"{CacheKeys.LessonsByCourseId(lesson.CourseId)}_ordered");

            return new LessonResponse
            {
                Id = createdLesson.Id,
                CourseId = createdLesson.CourseId,
                Title = createdLesson.Title,
                Slug = createdLesson.Slug,
                Content = createdLesson.Content,
                Template = createdLesson.Template,
                CreatedAt = createdLesson.CreatedAt,
                UpdatedAt = createdLesson.UpdatedAt,
                Language = createdLesson.Language,
                Order = createdLesson.Order,
                AdditionalResources = createdLesson.AdditionalResources,
                Tags = createdLesson.Tags,
                TestCases = createdLesson.TestConfig.TestCases
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating lesson {Title}", lesson.Title);
            throw;
        }
    }

    public async Task<LessonResponse> UpdateLesson(Lesson lesson)
    {
        if (lesson == null)
            throw new BadRequestException("Lesson cannot be null");

        if (string.IsNullOrEmpty(lesson.Id))
            throw new BadRequestException("Lesson ID cannot be null or empty");

        var validationErrors = await GetLessonValidationErrors(lesson);
        if (validationErrors.Count > 0)
            throw new BadRequestException($"Lesson validation failed: {string.Join(", ", validationErrors)}");

        try
        {
            var updatedLesson = await lessonRepository.Update(lesson);

            // Invalidate relevant caches
            await cachingService.RemoveAsync(CacheKeys.LessonById(lesson.Id));
            await cachingService.RemoveAsync(CacheKeys.LessonBySlug(lesson.Slug));
            await cachingService.RemoveAsync(CacheKeys.LessonsByCourseId(lesson.CourseId));
            await cachingService.RemoveAsync($"{CacheKeys.LessonsByCourseId(lesson.CourseId)}_ordered");

            return new LessonResponse
            {
                Id = updatedLesson.Id,
                CourseId = updatedLesson.CourseId,
                Title = updatedLesson.Title,
                Slug = updatedLesson.Slug,
                Content = updatedLesson.Content,
                Template = updatedLesson.Template,
                CreatedAt = updatedLesson.CreatedAt,
                UpdatedAt = updatedLesson.UpdatedAt,
                Language = updatedLesson.Language,
                Order = updatedLesson.Order,
                AdditionalResources = updatedLesson.AdditionalResources,
                Tags = updatedLesson.Tags,
                TestCases = updatedLesson.TestConfig.TestCases
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating lesson {Id}", lesson.Id);
            throw;
        }
    }

    public async Task<bool> DeleteLesson(string lessonId)
    {
        if (string.IsNullOrEmpty(lessonId))
            throw new BadRequestException("Lesson ID cannot be null or empty");

        try
        {
            var lesson = await lessonRepository.GetById(lessonId);
            if (lesson == null)
                throw new NotFoundException("Lesson", lessonId);

            var result = await lessonRepository.Delete(lessonId);

            if (result)
            {
                // Invalidate relevant caches
                await cachingService.RemoveAsync(CacheKeys.LessonById(lessonId));
                await cachingService.RemoveAsync(CacheKeys.LessonBySlug(lesson.Slug));
                await cachingService.RemoveAsync(CacheKeys.LessonsByCourseId(lesson.CourseId));
                await cachingService.RemoveAsync($"{CacheKeys.LessonsByCourseId(lesson.CourseId)}_ordered");
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting lesson {LessonId}", lessonId);
            throw;
        }
    }

    public async Task<bool> UpdateLessonStatus(string lessonId, string status)
    {
        if (string.IsNullOrEmpty(lessonId))
            throw new BadRequestException("Lesson ID cannot be null or empty");

        if (string.IsNullOrEmpty(status))
            throw new BadRequestException("Status cannot be null or empty");

        try
        {
            var lesson = await lessonRepository.GetById(lessonId);
            if (lesson == null)
                throw new NotFoundException("Lesson", lessonId);

            var result = await lessonRepository.UpdateStatus(lessonId, status);

            if (result)
            {
                // Invalidate relevant caches
                await cachingService.RemoveAsync(CacheKeys.LessonById(lessonId));
                await cachingService.RemoveAsync(CacheKeys.LessonBySlug(lesson.Slug));
                await cachingService.RemoveAsync(CacheKeys.LessonsByCourseId(lesson.CourseId));
                await cachingService.RemoveAsync($"{CacheKeys.LessonsByCourseId(lesson.CourseId)}_ordered");
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating lesson status {LessonId}", lessonId);
            throw;
        }
    }

    public async Task<bool> UpdateLessonOrder(string lessonId, int newOrder)
    {
        if (string.IsNullOrEmpty(lessonId))
            throw new BadRequestException("Lesson ID cannot be null or empty");

        try
        {
            var lesson = await lessonRepository.GetById(lessonId);
            if (lesson == null)
                throw new NotFoundException("Lesson", lessonId);

            var result = await lessonRepository.UpdateOrder(lessonId, newOrder);

            if (result)
            {
                // Invalidate relevant caches
                await cachingService.RemoveAsync(CacheKeys.LessonById(lessonId));
                await cachingService.RemoveAsync(CacheKeys.LessonsByCourseId(lesson.CourseId));
                await cachingService.RemoveAsync($"{CacheKeys.LessonsByCourseId(lesson.CourseId)}_ordered");
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating lesson order {LessonId}", lessonId);
            throw;
        }
    }

    public async Task<bool> ReorderLessons(string courseId, List<string> lessonIds)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        if (lessonIds == null || lessonIds.Count == 0)
            throw new BadRequestException("Lesson IDs cannot be null or empty");

        try
        {
            var result = await lessonRepository.ReorderLessons(courseId, lessonIds);

            if (result)
            {
                // Invalidate relevant caches
                await cachingService.RemoveAsync(CacheKeys.LessonsByCourseId(courseId));
                await cachingService.RemoveAsync($"{CacheKeys.LessonsByCourseId(courseId)}_ordered");

                // Invalidate individual lesson caches
                foreach (var lessonId in lessonIds)
                {
                    await cachingService.RemoveAsync(CacheKeys.LessonById(lessonId));
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reordering lessons for course {CourseId}", courseId);
            throw;
        }
    }

    public async Task<LessonResponse?> PreviewLesson(string lessonId)
    {
        if (string.IsNullOrEmpty(lessonId))
            throw new BadRequestException("Lesson ID cannot be null or empty");

        try
        {
            // Don't use cache for preview to always get fresh data
            var lesson = await lessonRepository.GetById(lessonId);
            if (lesson == null)
                return null;

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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error previewing lesson {LessonId}", lessonId);
            throw;
        }
    }

    public async Task<bool> ValidateLesson(Lesson lesson)
    {
        var errors = await GetLessonValidationErrors(lesson);
        return errors.Count == 0;
    }

    public async Task<List<string>> GetLessonValidationErrors(Lesson lesson)
    {
        if (lesson == null)
            return new List<string> { "Lesson cannot be null" };

        try
        {
            return await lessonRepository.GetValidationErrors(lesson);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating lesson {LessonId}", lesson.Id);
            return new List<string> { "Error occurred during lesson validation" };
        }
    }
}