using AscendDev.Core.Models.Courses;

namespace AscendDev.Core.Interfaces.Data;

public interface ICourseVersionRepository
{
    // Read operations
    Task<List<CourseVersion>?> GetVersionsByCourseId(string courseId);
    Task<CourseVersion?> GetVersionById(string versionId);
    Task<CourseVersion?> GetVersionByCourseIdAndVersion(string courseId, int versionNumber);
    Task<CourseVersion?> GetActiveVersionByCourseId(string courseId);
    Task<CourseVersion?> GetLatestVersionByCourseId(string courseId);

    // Write operations
    Task<CourseVersion> CreateVersion(CourseVersion version);
    Task<CourseVersion> UpdateVersion(CourseVersion version);
    Task<bool> DeleteVersion(string versionId);
    Task<bool> SetActiveVersion(string courseId, int versionNumber);

    // Version management
    Task<int> GetNextVersionNumber(string courseId);
    Task<bool> HasActiveVersion(string courseId);
    Task<List<CourseVersion>?> GetVersionHistory(string courseId);
}