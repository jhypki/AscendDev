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

        public AdminService(
            IUserRepository userRepository,
            ICourseRepository courseRepository,
            ILessonRepository lessonRepository,
            IUserProgressRepository userProgressRepository)
        {
            _userRepository = userRepository;
            _courseRepository = courseRepository;
            _lessonRepository = lessonRepository;
            _userProgressRepository = userProgressRepository;
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
            var query = _userRepository.GetQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(request.Search))
            {
                var searchLower = request.Search.ToLower();
                query = query.Where(u =>
                    u.Email.ToLower().Contains(searchLower) ||
                    (u.FirstName != null && u.FirstName.ToLower().Contains(searchLower)) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(searchLower)));
            }

            // Apply role filter - simplified since UserRole doesn't have Role navigation
            if (!string.IsNullOrEmpty(request.Role))
            {
                // This would need proper role lookup implementation
                // For now, skip role filtering
            }

            // Apply active status filter
            if (request.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == request.IsActive.Value);
            }

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            var users = query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(u => new UserManagementResponse
                {
                    Id = u.Id.ToString(),
                    Email = u.Email,
                    FirstName = u.FirstName ?? "",
                    LastName = u.LastName ?? "",
                    Roles = new List<string> { "User" }, // Simplified - would need proper role lookup
                    IsActive = u.IsActive,
                    LastLogin = u.LastLogin ?? DateTime.MinValue,
                    CreatedAt = u.CreatedAt,
                    CoursesEnrolled = 0, // Would need UserCourse relationship
                    LessonsCompleted = 0 // Would need UserProgress relationship
                })
                .ToList();

            return new PaginatedUserManagementResponse
            {
                Users = users,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = request.Page,
                PageSize = request.PageSize
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
    }
}