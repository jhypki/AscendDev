using AscendDev.Core.Exceptions;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.Courses;
using Microsoft.Extensions.Logging;

namespace AscendDev.Services.Services;

public class UserProgressService : IUserProgressService
{
    private readonly IUserProgressRepository _progressRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ILogger<UserProgressService> _logger;

    public UserProgressService(
        IUserProgressRepository progressRepository,
        ILessonRepository lessonRepository,
        ICourseRepository courseRepository,
        ILogger<UserProgressService> logger)
    {
        _progressRepository = progressRepository ?? throw new ArgumentNullException(nameof(progressRepository));
        _lessonRepository = lessonRepository ?? throw new ArgumentNullException(nameof(lessonRepository));
        _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<UserProgress>> GetUserProgressAsync(Guid userId)
    {
        try
        {
            return await _progressRepository.GetByUserId(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving progress for user {UserId}", userId);
            throw new InternalServerErrorException("Failed to retrieve user progress", ex.Message);
        }
    }

    public async Task<List<UserProgress>> GetLessonProgressAsync(string lessonId)
    {
        try
        {
            // Verify the lesson exists
            var lesson = await _lessonRepository.GetById(lessonId);
            if (lesson == null)
            {
                throw new NotFoundException($"Lesson with ID {lessonId} not found");
            }

            return await _progressRepository.GetByLessonId(lessonId);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving progress for lesson {LessonId}", lessonId);
            throw new InternalServerErrorException("Failed to retrieve lesson progress", ex.Message);
        }
    }

    public async Task<bool> HasUserCompletedLessonAsync(Guid userId, string lessonId)
    {
        try
        {
            return await _progressRepository.HasUserCompletedLesson(userId, lessonId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {UserId} completed lesson {LessonId}", userId, lessonId);
            throw new InternalServerErrorException("Failed to check lesson completion status", ex.Message);
        }
    }

    public async Task<UserProgress> MarkLessonAsCompletedAsync(Guid userId, string lessonId, string codeSolution)
    {
        try
        {
            // Verify the lesson exists
            var lesson = await _lessonRepository.GetById(lessonId);
            if (lesson == null)
            {
                throw new NotFoundException($"Lesson with ID {lessonId} not found");
            }

            // Check if the user has already completed this lesson
            var existingProgress = await _progressRepository.GetByUserAndLessonId(userId, lessonId);
            if (existingProgress != null)
            {
                // Update the existing progress with the new completion time and code solution
                existingProgress.CompletedAt = DateTime.UtcNow;
                existingProgress.CodeSolution = codeSolution;

                await _progressRepository.Update(existingProgress);
                return existingProgress;
            }

            // Create a new progress entry
            var progress = new UserProgress
            {
                UserId = userId,
                LessonId = lessonId,
                CompletedAt = DateTime.UtcNow,
                CodeSolution = codeSolution
            };

            return await _progressRepository.Create(progress);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking lesson {LessonId} as completed for user {UserId}", lessonId, userId);
            throw new InternalServerErrorException("Failed to mark lesson as completed", ex.Message);
        }
    }

    public async Task<int> GetCompletedLessonCountForCourseAsync(Guid userId, string courseId)
    {
        try
        {
            // Verify the course exists
            var course = await _courseRepository.GetById(courseId);
            if (course == null)
            {
                throw new NotFoundException($"Course with ID {courseId} not found");
            }

            return await _progressRepository.GetCompletedLessonCountForCourse(userId, courseId);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting completed lesson count for user {UserId} and course {CourseId}",
                userId, courseId);
            throw new InternalServerErrorException("Failed to get completed lesson count", ex.Message);
        }
    }

    public async Task<double> GetCourseCompletionPercentageAsync(Guid userId, string courseId)
    {
        try
        {
            // Verify the course exists
            var course = await _courseRepository.GetById(courseId);
            if (course == null)
            {
                throw new NotFoundException($"Course with ID {courseId} not found");
            }

            // Get all lessons for the course
            var lessons = await _lessonRepository.GetByCourseId(courseId);
            if (lessons == null || lessons.Count == 0)
            {
                return 0; // No lessons in the course
            }

            // Get the count of completed lessons
            var completedCount = await _progressRepository.GetCompletedLessonCountForCourse(userId, courseId);

            // Calculate the percentage
            return (double)completedCount / lessons.Count * 100;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating completion percentage for user {UserId} and course {CourseId}",
                userId, courseId);
            throw new InternalServerErrorException("Failed to calculate course completion percentage", ex.Message);
        }
    }
}