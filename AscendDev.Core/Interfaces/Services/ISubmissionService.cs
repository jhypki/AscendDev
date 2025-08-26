using AscendDev.Core.DTOs.Courses;
using AscendDev.Core.Models.Courses;

namespace AscendDev.Core.Interfaces.Services;

public interface ISubmissionService
{
    Task<SubmissionResponse?> GetSubmissionByIdAsync(int id);
    Task<IEnumerable<SubmissionResponse>> GetUserSubmissionsAsync(Guid userId);
    Task<IEnumerable<PublicSubmissionResponse>> GetPublicSubmissionsForLessonAsync(string lessonId, int limit = 50);
    Task<IEnumerable<PublicSubmissionResponse>> GetUserPublicSubmissionsAsync(Guid userId, int limit = 50);
    Task<int> CreateSubmissionAsync(Submission submission);
    Task UpdateSubmissionAsync(Submission submission);
    Task DeleteSubmissionAsync(int id);
    Task<Submission?> GetLatestSubmissionAsync(Guid userId, string lessonId);
    Task<int> GetUserSubmissionCountAsync(Guid userId);
    Task<int> GetUserPassedSubmissionCountAsync(Guid userId);
}