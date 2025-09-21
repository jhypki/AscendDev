using AscendDev.Core.Exceptions;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.Courses;
using AscendDev.Core.DTOs.Dashboard;
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

    public async Task<UserProgress> MarkLessonAsCompletedAsync(Guid userId, string lessonId, int submissionId)
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
                // Update the existing progress with the new completion time and submission ID
                existingProgress.CompletedAt = DateTime.UtcNow;
                existingProgress.SubmissionId = submissionId;

                await _progressRepository.Update(existingProgress);
                return existingProgress;
            }

            // Create a new progress entry
            var progress = new UserProgress
            {
                UserId = userId,
                LessonId = lessonId,
                CompletedAt = DateTime.UtcNow,
                SubmissionId = submissionId
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

    public async Task<DashboardStatsResponse> GetUserDashboardStatsAsync(Guid userId)
    {
        try
        {
            var userProgress = await _progressRepository.GetByUserId(userId);
            var allCourses = await _courseRepository.GetAll();

            // Get courses the user has progress in
            var userCourseIds = userProgress.Select(p => p.LessonId).ToList();
            var userCourses = new List<Course>();

            foreach (var courseId in userCourseIds.Distinct())
            {
                var lesson = await _lessonRepository.GetById(courseId);
                if (lesson != null)
                {
                    var course = await _courseRepository.GetById(lesson.CourseId);
                    if (course != null && !userCourses.Any(c => c.Id == course.Id))
                    {
                        userCourses.Add(course);
                    }
                }
            }

            var totalCourses = userCourses.Count;
            var completedCourses = 0;
            var inProgressCourses = 0;
            var totalLessons = 0;

            foreach (var course in userCourses)
            {
                var lessons = await _lessonRepository.GetByCourseId(course.Id);
                totalLessons += lessons?.Count ?? 0;

                var completedCount = await _progressRepository.GetCompletedLessonCountForCourse(userId, course.Id);
                var totalLessonCount = lessons?.Count ?? 0;

                if (completedCount == totalLessonCount && totalLessonCount > 0)
                {
                    completedCourses++;
                }
                else if (completedCount > 0)
                {
                    inProgressCourses++;
                }
            }

            var completedLessons = userProgress.Count;
            var streakDays = await GetUserStreakDaysAsync(userId);
            var recentActivity = await GetUserRecentActivityAsync(userId, 5);

            return new DashboardStatsResponse
            {
                TotalCourses = totalCourses,
                CompletedCourses = completedCourses,
                InProgressCourses = inProgressCourses,
                TotalLessons = totalLessons,
                CompletedLessons = completedLessons,
                StreakDays = streakDays,
                TotalStudyTime = completedLessons * 15, // Estimate 15 minutes per lesson
                RecentActivity = recentActivity
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard stats for user {UserId}", userId);
            throw new InternalServerErrorException("Failed to get dashboard statistics", ex.Message);
        }
    }

    public async Task<List<UserProgressResponse>> GetUserCoursesProgressAsync(Guid userId)
    {
        try
        {
            var userProgress = await _progressRepository.GetByUserId(userId);
            var courseProgressList = new List<UserProgressResponse>();

            // Group progress by course
            var courseGroups = new Dictionary<string, List<UserProgress>>();

            foreach (var progress in userProgress)
            {
                var lesson = await _lessonRepository.GetById(progress.LessonId);
                if (lesson != null)
                {
                    if (!courseGroups.ContainsKey(lesson.CourseId))
                    {
                        courseGroups[lesson.CourseId] = new List<UserProgress>();
                    }
                    courseGroups[lesson.CourseId].Add(progress);
                }
            }

            foreach (var courseGroup in courseGroups)
            {
                var course = await _courseRepository.GetById(courseGroup.Key);
                if (course != null)
                {
                    var lessons = await _lessonRepository.GetByCourseId(course.Id);
                    var totalLessons = lessons?.Count ?? 0;
                    var completedLessons = courseGroup.Value.Count;
                    var completionPercentage = totalLessons > 0 ? (int)Math.Round((double)completedLessons / totalLessons * 100) : 0;
                    var lastAccessed = courseGroup.Value.Max(p => p.CompletedAt);

                    courseProgressList.Add(new UserProgressResponse
                    {
                        CourseId = course.Id,
                        CourseTitle = course.Title,
                        TotalLessons = totalLessons,
                        CompletedLessons = completedLessons,
                        CompletionPercentage = completionPercentage,
                        LastAccessed = lastAccessed
                    });
                }
            }

            return courseProgressList.OrderByDescending(p => p.LastAccessed).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user courses progress for user {UserId}", userId);
            throw new InternalServerErrorException("Failed to get user courses progress", ex.Message);
        }
    }

    public async Task<List<LearningStreakResponse>> GetUserLearningStreakAsync(Guid userId, int days = 30)
    {
        try
        {
            var userProgress = await _progressRepository.GetByUserId(userId);
            var streakData = new List<LearningStreakResponse>();
            var today = DateTime.UtcNow.Date;

            for (int i = days - 1; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var completedOnDate = userProgress.Count(p => p.CompletedAt.Date == date);

                streakData.Add(new LearningStreakResponse
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    Completed = completedOnDate
                });
            }

            return streakData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting learning streak for user {UserId}", userId);
            throw new InternalServerErrorException("Failed to get learning streak", ex.Message);
        }
    }

    public async Task<int> GetTotalCompletedLessonsAsync(Guid userId)
    {
        try
        {
            var userProgress = await _progressRepository.GetByUserId(userId);
            return userProgress.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total completed lessons for user {UserId}", userId);
            throw new InternalServerErrorException("Failed to get total completed lessons", ex.Message);
        }
    }

    public async Task<int> GetUserStreakDaysAsync(Guid userId)
    {
        try
        {
            var userProgress = await _progressRepository.GetByUserId(userId);
            if (!userProgress.Any()) return 0;

            var completionDates = userProgress
                .Select(p => p.CompletedAt.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            var streak = 0;
            var currentDate = DateTime.UtcNow.Date;

            foreach (var date in completionDates)
            {
                if (date == currentDate || date == currentDate.AddDays(-streak))
                {
                    streak++;
                    currentDate = date.AddDays(-1);
                }
                else
                {
                    break;
                }
            }

            return streak;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting streak days for user {UserId}", userId);
            throw new InternalServerErrorException("Failed to get streak days", ex.Message);
        }
    }

    public async Task<List<RecentActivityResponse>> GetUserRecentActivityAsync(Guid userId, int limit = 10)
    {
        try
        {
            var userProgress = await _progressRepository.GetByUserId(userId);
            var recentActivity = new List<RecentActivityResponse>();

            var recentProgress = userProgress
                .OrderByDescending(p => p.CompletedAt)
                .Take(limit);

            foreach (var progress in recentProgress)
            {
                var lesson = await _lessonRepository.GetById(progress.LessonId);
                if (lesson != null)
                {
                    var course = await _courseRepository.GetById(lesson.CourseId);

                    recentActivity.Add(new RecentActivityResponse
                    {
                        Id = progress.Id.ToString(),
                        Type = "lesson_completed",
                        Title = lesson.Title,
                        CourseTitle = course?.Title,
                        Timestamp = progress.CompletedAt
                    });
                }
            }

            return recentActivity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent activity for user {UserId}", userId);
            throw new InternalServerErrorException("Failed to get recent activity", ex.Message);
        }
    }
}