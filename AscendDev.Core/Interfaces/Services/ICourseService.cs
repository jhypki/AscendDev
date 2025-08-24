using AscendDev.Core.DTOs.Courses;
using AscendDev.Core.Models.Courses;

namespace AscendDev.Core.Interfaces.Services;

public interface ICourseService
{
    // Read operations
    Task<List<CourseResponse>?> GetAllCourses();
    Task<CourseResponse?> GetCourseById(string courseId);
    Task<CourseResponse?> GetCourseBySlug(string slug);
    Task<List<CourseResponse>?> GetCoursesByLanguage(string language);
    Task<List<CourseResponse>?> GetCoursesByTags(List<string> tags);
    Task<List<CourseResponse>?> GetCoursesByStatus(string status);
    Task<List<CourseResponse>?> GetPublishedCourses();
    Task<List<CourseResponse>?> GetCoursesByCreator(string creatorId);

    // CRUD operations
    Task<CourseResponse> CreateCourse(CreateCourseRequest request, string userId);
    Task<CourseResponse?> UpdateCourse(string courseId, UpdateCourseRequest request, string userId);
    Task<bool> DeleteCourse(string courseId, string userId);

    // Versioning operations
    Task<CourseResponse> CreateCourseVersion(string courseId, string userId);
    Task<List<CourseResponse>?> GetCourseVersions(string parentCourseId);
    Task<CourseResponse?> GetLatestCourseVersion(string parentCourseId);

    // Publishing workflow
    Task<CourseResponse?> PublishCourse(string courseId, string userId);
    Task<CourseResponse?> UnpublishCourse(string courseId, string userId);
    Task<CourseResponse?> PreviewCourse(string courseId, string userId);

    // Validation operations
    Task<CourseResponse?> ValidateCourse(string courseId, string userId);
    Task<List<string>> GetCourseValidationErrors(string courseId);

    // Analytics operations
    Task<CourseAnalyticsResponse?> GetCourseAnalytics(string courseId);
    Task<bool> IncrementCourseViews(string courseId);
    Task<bool> TrackCourseEnrollment(string courseId);

    // Search and filtering
    Task<List<CourseResponse>?> SearchCourses(string searchTerm, string? language = null, List<string>? tags = null, string? status = null);
    Task<List<CourseResponse>?> GetPaginatedCourses(int page, int pageSize, string? sortBy = null, bool ascending = true);
}