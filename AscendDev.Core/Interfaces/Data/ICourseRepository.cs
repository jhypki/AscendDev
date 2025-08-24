using AscendDev.Core.DTOs.Courses;
using AscendDev.Core.Models.Courses;

namespace AscendDev.Core.Interfaces.Data;

public interface ICourseRepository
{
    // Read operations
    Task<List<Course>?> GetAll();
    Task<Course?> GetById(string courseId);
    Task<Course?> GetBySlug(string slug);
    Task<Course?> GetByLanguage(string language);
    Task<List<Course>?> GetByTag(string tag);
    Task<List<Course>?> GetByStatus(string status);
    Task<List<Course>?> GetPublished();
    Task<List<Course>?> GetByCreator(string creatorId);

    // CRUD operations
    Task<Course> Create(Course course);
    Task<Course?> Update(string courseId, Course course);
    Task<bool> Delete(string courseId);
    Task<bool> SoftDelete(string courseId);

    // Versioning operations
    Task<Course> CreateVersion(string courseId, string userId);
    Task<List<Course>?> GetVersions(string parentCourseId);
    Task<Course?> GetLatestVersion(string parentCourseId);

    // Publishing operations
    Task<bool> Publish(string courseId, string userId);
    Task<bool> Unpublish(string courseId, string userId);

    // Validation operations
    Task<bool> Validate(string courseId, string userId, List<string> validationErrors);
    Task<bool> MarkAsValidated(string courseId, string userId);

    // Analytics operations
    Task<CourseAnalyticsResponse?> GetAnalytics(string courseId);
    Task<bool> IncrementViewCount(string courseId);
    Task<bool> IncrementEnrollmentCount(string courseId);
    Task<bool> UpdateRating(string courseId, double rating, int ratingCount);

    // Search and filtering
    Task<List<Course>?> Search(string searchTerm, string? language = null, List<string>? tags = null, string? status = null);
    Task<List<Course>?> GetPaginated(int page, int pageSize, string? sortBy = null, bool ascending = true);
}