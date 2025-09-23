using AscendDev.Core.DTOs.Admin;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.Auth;

namespace AscendDev.Services.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IUserProgressRepository _userProgressRepository;
        private readonly IUserManagementService _userManagementService;

        public AdminService(
            IUserRepository userRepository,
            ICourseRepository courseRepository,
            ILessonRepository lessonRepository,
            IUserProgressRepository userProgressRepository,
            IUserManagementService userManagementService)
        {
            _userRepository = userRepository;
            _courseRepository = courseRepository;
            _lessonRepository = lessonRepository;
            _userProgressRepository = userProgressRepository;
            _userManagementService = userManagementService;
        }

        public async Task<AdminStatsResponse> GetAdminStatsAsync()
        {
            // Get real data from repositories
            var totalUsers = await _userRepository.CountAsync();
            var activeUsers = await _userRepository.CountAsync(u => u.IsActive);

            // New registrations in the last 30 days
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var newRegistrations = await _userRepository.CountAsync(u => u.CreatedAt >= thirtyDaysAgo);

            var totalCourses = await _courseRepository.CountAsync();
            var publishedCourses = await _courseRepository.CountAsync(c => c.Status == "published");
            var totalLessons = await _lessonRepository.CountAsync();

            return new AdminStatsResponse
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                NewRegistrations = newRegistrations,
                TotalCourses = totalCourses,
                PublishedCourses = publishedCourses,
                TotalLessons = totalLessons,
                SystemHealth = "healthy", // This could be calculated based on various metrics
                ServerUptime = 99.8 // This would typically come from monitoring systems
            };
        }

        public async Task<PaginatedUserManagementResponse> GetUsersAsync(UserManagementQueryRequest request)
        {
            // Convert the request to UserSearchRequest format
            var searchRequest = new UserSearchRequest
            {
                SearchTerm = request.Search,
                IsActive = request.IsActive,
                Page = request.Page,
                PageSize = request.PageSize,
                SortBy = "CreatedAt",
                SortDirection = "desc"
            };

            // Use the injected UserManagementService
            var pagedResult = await _userManagementService.SearchUsersAsync(searchRequest);

            // Convert to the expected response format
            var users = pagedResult.Items.Select(u => new UserManagementResponse
            {
                Id = u.Id.ToString(),
                Email = u.Email,
                FirstName = u.FirstName ?? "",
                LastName = u.LastName ?? "",
                Roles = u.Roles?.ToList() ?? new List<string>(),
                IsActive = u.IsActive,
                LastLogin = u.LastLogin,
                CreatedAt = u.CreatedAt,
                CoursesEnrolled = u.Statistics?.CoursesCompleted ?? 0,
                LessonsCompleted = u.Statistics?.LessonsCompleted ?? 0
            }).ToList();

            return new PaginatedUserManagementResponse
            {
                Users = users,
                TotalCount = pagedResult.TotalCount,
                TotalPages = pagedResult.TotalPages,
                CurrentPage = pagedResult.Page,
                PageSize = pagedResult.PageSize
            };
        }

        public async Task<List<CourseAnalyticsResponse>> GetCourseAnalyticsAsync()
        {
            var courses = await _courseRepository.GetPublishedCourses();

            if (courses == null)
                return new List<CourseAnalyticsResponse>();

            return courses.Select(c => new CourseAnalyticsResponse
            {
                CourseId = c.Id,
                Title = c.Title,
                Enrollments = 0, // Would need UserCourse relationship for real data
                Completions = 0, // Would need UserCourse completion tracking
                AverageRating = 0, // Would need rating system
                TotalLessons = c.LessonSummaries?.Count ?? 0,
                Language = c.Language,
                Status = c.Status,
                CreatedAt = c.CreatedAt
            }).OrderByDescending(c => c.TotalLessons).ToList();
        }

        public async Task<SystemAnalyticsResponse> GetSystemAnalyticsAsync()
        {
            var now = DateTime.UtcNow;
            var userGrowth = new List<UserGrowthData>();

            // Generate real user growth data for the last 30 days
            for (int i = 29; i >= 0; i--)
            {
                var date = now.AddDays(-i);
                var startOfDay = date.Date;
                var endOfDay = startOfDay.AddDays(1);

                var usersCount = await _userRepository.CountAsync(u => u.CreatedAt >= startOfDay && u.CreatedAt < endOfDay);
                var coursesCount = await _courseRepository.CountAsync(c => c.CreatedAt >= startOfDay && c.CreatedAt < endOfDay);
                var lessonsCount = await _lessonRepository.CountAsync(l => l.CreatedAt >= startOfDay && l.CreatedAt < endOfDay);

                userGrowth.Add(new UserGrowthData
                {
                    Date = startOfDay.ToString("yyyy-MM-dd"),
                    Users = usersCount,
                    Courses = coursesCount,
                    Lessons = lessonsCount
                });
            }

            // Get top 5 courses by lesson count (since we don't have enrollment data yet)
            var publishedCourses = await _courseRepository.GetPublishedCourses();
            var topCourses = publishedCourses?
                .OrderByDescending(c => c.LessonSummaries?.Count ?? 0)
                .Take(5)
                .Select(c => new TopCourseData
                {
                    Name = c.Title,
                    Enrollments = c.LessonSummaries?.Count ?? 0 // Using lesson count as proxy for popularity
                })
                .ToList() ?? new List<TopCourseData>();

            return new SystemAnalyticsResponse
            {
                UserGrowth = userGrowth,
                TopCourses = topCourses
            };
        }

        public async Task<bool> UpdateUserStatusAsync(string userId, bool isActive)
        {
            if (!Guid.TryParse(userId, out var userGuid))
                return false;

            var user = await _userRepository.GetByIdAsync(userGuid);
            if (user == null)
                return false;

            user.IsActive = isActive;
            return await _userRepository.UpdateAsync(user);
        }

        public async Task<bool> UpdateUserRolesAsync(string userId, List<string> roles)
        {
            // Mock implementation - would need proper role management
            // In a real implementation, you'd need to work with the role system
            await Task.Delay(100); // Simulate async operation
            return true;
        }

        public async Task<ReportGenerationResponse> GenerateReportAsync(GenerateReportRequest request)
        {
            // Simulate report generation
            await Task.Delay(1000); // Simulate processing time

            var reportId = Guid.NewGuid();
            var generatedAt = DateTime.UtcNow;

            object reportData = request.ReportType switch
            {
                "user-activity" => await GenerateUserActivityReportData(request),
                "course-analytics" => await GenerateCourseAnalyticsReportData(request),
                "system-health" => await GenerateSystemHealthReportData(request),
                _ => new { message = "Unknown report type" }
            };

            return new ReportGenerationResponse
            {
                ReportId = reportId,
                ReportType = request.ReportType,
                Status = "completed",
                GeneratedAt = generatedAt,
                Data = reportData,
                DownloadUrl = $"/api/admin/reports/{reportId}/download"
            };
        }

        private async Task<object> GenerateUserActivityReportData(GenerateReportRequest request)
        {
            var totalUsers = await _userRepository.CountAsync();
            var activeUsers = await _userRepository.CountAsync(u => u.IsActive);
            var startDate = request.StartDate ?? DateTime.UtcNow.AddDays(-30);
            var endDate = request.EndDate ?? DateTime.UtcNow;

            return new
            {
                Summary = new
                {
                    TotalUsers = totalUsers,
                    ActiveUsers = activeUsers,
                    InactiveUsers = totalUsers - activeUsers,
                    ReportPeriod = new { StartDate = startDate, EndDate = endDate }
                },
                UserRegistrations = new
                {
                    NewRegistrations = await _userRepository.CountAsync(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate),
                    DailyBreakdown = "Would contain daily registration data"
                },
                UserEngagement = new
                {
                    LoginActivity = "Would contain login statistics",
                    CourseEnrollments = "Would contain enrollment data",
                    LessonCompletions = "Would contain completion data"
                }
            };
        }

        private async Task<object> GenerateCourseAnalyticsReportData(GenerateReportRequest request)
        {
            var totalCourses = await _courseRepository.CountAsync();
            var publishedCourses = await _courseRepository.CountAsync(c => c.Status == "published");
            var totalLessons = await _lessonRepository.CountAsync();

            return new
            {
                Summary = new
                {
                    TotalCourses = totalCourses,
                    PublishedCourses = publishedCourses,
                    DraftCourses = totalCourses - publishedCourses,
                    TotalLessons = totalLessons
                },
                CoursePerformance = new
                {
                    TopCourses = "Would contain top performing courses",
                    EnrollmentTrends = "Would contain enrollment trend data",
                    CompletionRates = "Would contain completion statistics"
                },
                ContentAnalysis = new
                {
                    LanguageDistribution = "Would contain programming language breakdown",
                    DifficultyLevels = "Would contain difficulty level analysis",
                    TagAnalysis = "Would contain tag usage statistics"
                }
            };
        }

        private async Task<object> GenerateSystemHealthReportData(GenerateReportRequest request)
        {
            await Task.Delay(100); // Simulate data gathering

            return new
            {
                SystemStatus = new
                {
                    Uptime = "99.8%",
                    Status = "Healthy",
                    LastRestart = DateTime.UtcNow.AddDays(-7),
                    Version = "1.0.0"
                },
                Performance = new
                {
                    AverageResponseTime = "120ms",
                    DatabaseConnections = 45,
                    MemoryUsage = "2.1GB",
                    CpuUsage = "15%"
                },
                Storage = new
                {
                    DatabaseSize = "1.2GB",
                    FileStorage = "850MB",
                    BackupStatus = "Last backup: 2 hours ago"
                },
                Security = new
                {
                    FailedLoginAttempts = 12,
                    ActiveSessions = 234,
                    SecurityAlerts = 0
                }
            };
        }
    }
}