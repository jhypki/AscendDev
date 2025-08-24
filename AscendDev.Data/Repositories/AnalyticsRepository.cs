using AscendDev.Core.DTOs.Admin;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Admin;
using Dapper;
using System.Text.Json;

namespace AscendDev.Data.Repositories;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly ISqlExecutor _sqlExecutor;

    public AnalyticsRepository(ISqlExecutor sqlExecutor)
    {
        _sqlExecutor = sqlExecutor;
    }

    public async Task<DashboardStatisticsDto> GetDashboardStatisticsAsync()
    {
        // Try to get cached statistics first
        var cachedStats = await GetDashboardStatisticAsync("dashboard_overview");
        if (cachedStats != null && cachedStats.ExpiresAt > DateTime.UtcNow)
        {
            return JsonSerializer.Deserialize<DashboardStatisticsDto>(
                JsonSerializer.Serialize(cachedStats.StatisticValue)) ?? new DashboardStatisticsDto();
        }

        // Calculate fresh statistics
        var stats = new DashboardStatisticsDto();

        // Get basic counts
        var basicStatsSql = @"
            SELECT 
                (SELECT COUNT(*) FROM users) as total_users,
                (SELECT COUNT(*) FROM users WHERE is_active = true AND last_login >= NOW() - INTERVAL '1 day') as active_users_today,
                (SELECT COUNT(*) FROM users WHERE is_active = true AND last_login >= NOW() - INTERVAL '7 days') as active_users_this_week,
                (SELECT COUNT(*) FROM users WHERE is_active = true AND last_login >= NOW() - INTERVAL '30 days') as active_users_this_month,
                (SELECT COUNT(*) FROM user_progress WHERE completed_at >= CURRENT_DATE) as lessons_completed_today,
                (SELECT COUNT(*) FROM user_progress WHERE completed_at >= CURRENT_DATE - INTERVAL '7 days') as lessons_completed_this_week,
                (SELECT COUNT(*) FROM user_progress WHERE completed_at >= CURRENT_DATE - INTERVAL '30 days') as lessons_completed_this_month,
                (SELECT COUNT(*) FROM courses WHERE status = 'published') as courses_published,
                (SELECT COUNT(*) FROM lessons) as total_lessons,
                (SELECT COUNT(*) FROM discussions) as total_discussions";

        var basicStats = await _sqlExecutor.QueryFirstAsync<dynamic>(basicStatsSql);

        stats.TotalUsers = (int)basicStats.total_users;
        stats.ActiveUsersToday = (int)basicStats.active_users_today;
        stats.ActiveUsersThisWeek = (int)basicStats.active_users_this_week;
        stats.ActiveUsersThisMonth = (int)basicStats.active_users_this_month;
        stats.LessonsCompletedToday = (int)basicStats.lessons_completed_today;
        stats.LessonsCompletedThisWeek = (int)basicStats.lessons_completed_this_week;
        stats.LessonsCompletedThisMonth = (int)basicStats.lessons_completed_this_month;
        stats.CoursesPublished = (int)basicStats.courses_published;
        stats.TotalLessons = (int)basicStats.total_lessons;
        stats.TotalDiscussions = (int)basicStats.total_discussions;

        // Get average session duration
        var sessionDurationSql = @"
            SELECT AVG(EXTRACT(EPOCH FROM (COALESCE(ended_at, NOW()) - started_at)) / 60) as avg_duration
            FROM user_sessions 
            WHERE started_at >= NOW() - INTERVAL '30 days'";

        var avgDuration = await _sqlExecutor.QueryFirstOrDefaultAsync<double?>(sessionDurationSql);
        stats.AverageSessionDurationMinutes = avgDuration ?? 0;

        // Get popular courses
        stats.PopularCourses = await GetPopularCoursesAsync(5);

        // Cache the results
        await UpdateDashboardStatisticAsync("dashboard_overview", stats, DateTime.UtcNow.AddMinutes(15));

        return stats;
    }

    public async Task<bool> UpdateDashboardStatisticAsync(string key, object value, DateTime? expiresAt = null)
    {
        var sql = @"
            INSERT INTO dashboard_statistics (id, statistic_key, statistic_value, last_updated, expires_at)
            VALUES (gen_random_uuid(), @key, @value::jsonb, NOW(), @expiresAt)
            ON CONFLICT (statistic_key) 
            DO UPDATE SET 
                statistic_value = @value::jsonb,
                last_updated = NOW(),
                expires_at = @expiresAt";

        var jsonValue = JsonSerializer.Serialize(value);
        return await _sqlExecutor.ExecuteAsync(sql, new { key, value = jsonValue, expiresAt }) > 0;
    }

    public async Task<DashboardStatistic?> GetDashboardStatisticAsync(string key)
    {
        var sql = @"
            SELECT id, statistic_key, statistic_value, last_updated, expires_at
            FROM dashboard_statistics
            WHERE statistic_key = @key";

        var result = await _sqlExecutor.QueryFirstOrDefaultAsync<dynamic>(sql, new { key });
        if (result == null) return null;

        return new DashboardStatistic
        {
            Id = result.id,
            StatisticKey = result.statistic_key,
            StatisticValue = JsonSerializer.Deserialize<Dictionary<string, object>>(result.statistic_value.ToString()) ?? new Dictionary<string, object>(),
            LastUpdated = result.last_updated,
            ExpiresAt = result.expires_at
        };
    }

    public async Task<bool> RefreshDashboardStatisticsAsync()
    {
        // Clear all cached statistics
        await _sqlExecutor.ExecuteAsync("DELETE FROM dashboard_statistics");

        // Regenerate dashboard statistics
        await GetDashboardStatisticsAsync();

        return true;
    }

    public async Task<UserEngagementReportDto> GetUserEngagementReportAsync(DateTime startDate, DateTime endDate)
    {
        var sql = @"
            SELECT 
                COUNT(DISTINCT u.id) as total_users,
                COUNT(DISTINCT CASE WHEN u.last_login >= @startDate THEN u.id END) as active_users,
                COUNT(DISTINCT CASE WHEN u.last_login < @startDate OR u.last_login IS NULL THEN u.id END) as inactive_users
            FROM users u
            WHERE u.created_at <= @endDate";

        var stats = await _sqlExecutor.QueryFirstAsync<dynamic>(sql, new { startDate, endDate });

        var totalUsers = (int)stats.total_users;
        var activeUsers = (int)stats.active_users;
        var inactiveUsers = (int)stats.inactive_users;

        var engagementRate = totalUsers > 0 ? (double)activeUsers / totalUsers * 100 : 0;

        return new UserEngagementReportDto
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            InactiveUsers = inactiveUsers,
            EngagementRate = engagementRate,
            TopEngagedUsers = new List<UserEngagementDetailDto>(),
            LeastEngagedUsers = new List<UserEngagementDetailDto>()
        };
    }

    public async Task<List<UserActivityTrendDto>> GetUserActivityTrendAsync(DateTime startDate, DateTime endDate)
    {
        var sql = @"
            SELECT 
                DATE(ual.created_at) as date,
                COUNT(DISTINCT ual.user_id) as active_users,
                COUNT(CASE WHEN ual.activity_type = 'user_registered' THEN 1 END) as new_registrations,
                COUNT(CASE WHEN ual.activity_type = 'login' THEN 1 END) as login_count
            FROM user_activity_logs ual
            WHERE ual.created_at >= @startDate AND ual.created_at <= @endDate
            GROUP BY DATE(ual.created_at)
            ORDER BY DATE(ual.created_at)";

        var trends = await _sqlExecutor.QueryAsync<UserActivityTrendDto>(sql, new { startDate, endDate });
        return trends.ToList();
    }

    public async Task<int> GetActiveUsersCountAsync(DateTime startDate, DateTime endDate)
    {
        var sql = @"
            SELECT COUNT(DISTINCT user_id)
            FROM user_activity_logs
            WHERE created_at >= @startDate AND created_at <= @endDate";

        return await _sqlExecutor.QueryFirstAsync<int>(sql, new { startDate, endDate });
    }

    public async Task<int> GetNewRegistrationsCountAsync(DateTime startDate, DateTime endDate)
    {
        var sql = @"
            SELECT COUNT(*)
            FROM users
            WHERE created_at >= @startDate AND created_at <= @endDate";

        return await _sqlExecutor.QueryFirstAsync<int>(sql, new { startDate, endDate });
    }

    public async Task<List<CourseAnalyticsDto>> GetCourseAnalyticsAsync(DateTime startDate, DateTime endDate)
    {
        var sql = @"
            SELECT 
                c.id as course_id,
                c.title,
                c.language,
                COUNT(DISTINCT up.user_id) as total_enrollments,
                COUNT(DISTINCT CASE WHEN course_progress.completion_rate = 100 THEN up.user_id END) as completed_enrollments,
                COALESCE(AVG(course_progress.completion_rate), 0) as completion_rate
            FROM courses c
            LEFT JOIN lessons l ON c.id = l.course_id
            LEFT JOIN user_progress up ON l.id = up.lesson_id 
                AND up.completed_at >= @startDate AND up.completed_at <= @endDate
            LEFT JOIN (
                SELECT 
                    c2.id as course_id,
                    up2.user_id,
                    (COUNT(up2.id) * 100.0 / COUNT(l2.id)) as completion_rate
                FROM courses c2
                JOIN lessons l2 ON c2.id = l2.course_id
                LEFT JOIN user_progress up2 ON l2.id = up2.lesson_id
                GROUP BY c2.id, up2.user_id
            ) course_progress ON c.id = course_progress.course_id AND up.user_id = course_progress.user_id
            WHERE c.status = 'published'
            GROUP BY c.id, c.title, c.language
            ORDER BY total_enrollments DESC";

        var analytics = await _sqlExecutor.QueryAsync<CourseAnalyticsDto>(sql, new { startDate, endDate });
        return analytics.ToList();
    }

    public async Task<CourseAnalyticsDto?> GetCourseAnalyticsAsync(string courseId, DateTime startDate, DateTime endDate)
    {
        var sql = @"
            SELECT 
                c.id as course_id,
                c.title,
                c.language,
                COUNT(DISTINCT up.user_id) as total_enrollments,
                COUNT(DISTINCT CASE WHEN course_progress.completion_rate = 100 THEN up.user_id END) as completed_enrollments,
                COALESCE(AVG(course_progress.completion_rate), 0) as completion_rate
            FROM courses c
            LEFT JOIN lessons l ON c.id = l.course_id
            LEFT JOIN user_progress up ON l.id = up.lesson_id 
                AND up.completed_at >= @startDate AND up.completed_at <= @endDate
            LEFT JOIN (
                SELECT 
                    c2.id as course_id,
                    up2.user_id,
                    (COUNT(up2.id) * 100.0 / COUNT(l2.id)) as completion_rate
                FROM courses c2
                JOIN lessons l2 ON c2.id = l2.course_id
                LEFT JOIN user_progress up2 ON l2.id = up2.lesson_id
                WHERE c2.id = @courseId
                GROUP BY c2.id, up2.user_id
            ) course_progress ON c.id = course_progress.course_id AND up.user_id = course_progress.user_id
            WHERE c.id = @courseId
            GROUP BY c.id, c.title, c.language";

        return await _sqlExecutor.QueryFirstOrDefaultAsync<CourseAnalyticsDto>(sql, new { courseId, startDate, endDate });
    }

    public async Task<List<PopularCourseDto>> GetPopularCoursesAsync(int limit = 10)
    {
        var sql = @"
            SELECT 
                c.id,
                c.title,
                c.language,
                COUNT(DISTINCT up.user_id) as completions,
                COUNT(DISTINCT recent_progress.user_id) as active_students,
                0.0 as average_rating
            FROM courses c
            LEFT JOIN lessons l ON c.id = l.course_id
            LEFT JOIN user_progress up ON l.id = up.lesson_id
            LEFT JOIN user_progress recent_progress ON l.id = recent_progress.lesson_id 
                AND recent_progress.completed_at >= NOW() - INTERVAL '30 days'
            WHERE c.status = 'published'
            GROUP BY c.id, c.title, c.language
            ORDER BY completions DESC, active_students DESC
            LIMIT @limit";

        var courses = await _sqlExecutor.QueryAsync<PopularCourseDto>(sql, new { limit });
        return courses.ToList();
    }

    public async Task<List<LessonCompletionTrendDto>> GetLessonCompletionTrendAsync(DateTime startDate, DateTime endDate)
    {
        var sql = @"
            SELECT 
                DATE(up.completed_at) as date,
                COUNT(*) as completed_lessons,
                COUNT(DISTINCT up.user_id) as unique_users
            FROM user_progress up
            WHERE up.completed_at >= @startDate AND up.completed_at <= @endDate
            GROUP BY DATE(up.completed_at)
            ORDER BY DATE(up.completed_at)";

        var trends = await _sqlExecutor.QueryAsync<LessonCompletionTrendDto>(sql, new { startDate, endDate });
        return trends.ToList();
    }

    public async Task<SystemHealthDto> GetSystemHealthAsync()
    {
        var sql = @"
            SELECT 
                COUNT(*) as active_sessions,
                0.0 as average_response_time,
                0.0 as error_rate,
                0 as total_requests
            FROM user_sessions
            WHERE is_active = true";

        var health = await _sqlExecutor.QueryFirstAsync<SystemHealthDto>(sql);
        health.LastUpdated = DateTime.UtcNow;
        health.EndpointHealth = new List<EndpointHealthDto>();

        return health;
    }

    public async Task<int> GetActiveSessionsCountAsync()
    {
        var sql = "SELECT COUNT(*) FROM user_sessions WHERE is_active = true";
        return await _sqlExecutor.QueryFirstAsync<int>(sql);
    }

    public async Task<List<EndpointHealthDto>> GetEndpointHealthAsync(DateTime startDate, DateTime endDate)
    {
        // This would typically come from application logs or monitoring systems
        // For now, return empty list as we don't have endpoint monitoring data
        return new List<EndpointHealthDto>();
    }

    public async Task<byte[]> GenerateReportAsync(ReportRequest request)
    {
        // This is a simplified implementation
        // In a real application, you would use a reporting library like iTextSharp for PDF,
        // EPPlus for Excel, or CsvHelper for CSV generation

        var data = await GetCustomReportDataAsync(
            "SELECT * FROM users WHERE created_at >= @startDate AND created_at <= @endDate",
            new Dictionary<string, object>
            {
                { "startDate", request.StartDate },
                { "endDate", request.EndDate }
            });

        // For now, return a simple CSV representation
        var csv = "Id,Email,Username,CreatedAt\n";
        foreach (var row in data)
        {
            csv += string.Join(",", row.Values) + "\n";
        }

        return System.Text.Encoding.UTF8.GetBytes(csv);
    }

    public async Task<List<Dictionary<string, object>>> GetCustomReportDataAsync(string query, Dictionary<string, object>? parameters = null)
    {
        var dynamicParams = new DynamicParameters();
        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                dynamicParams.Add(param.Key, param.Value);
            }
        }

        var results = await _sqlExecutor.QueryAsync<dynamic>(query, dynamicParams);

        return results.Select(row =>
        {
            var dict = new Dictionary<string, object>();
            foreach (var prop in (IDictionary<string, object>)row)
            {
                dict[prop.Key] = prop.Value ?? "";
            }
            return dict;
        }).ToList();
    }
}