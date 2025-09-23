using AscendDev.Core.DTOs.Courses;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.Courses;

namespace AscendDev.Services.Services;

public class SubmissionService : ISubmissionService
{
    private readonly ISubmissionRepository _submissionRepository;

    public SubmissionService(ISubmissionRepository submissionRepository)
    {
        _submissionRepository = submissionRepository;
    }

    public async Task<SubmissionResponse?> GetSubmissionByIdAsync(int id)
    {
        var submission = await _submissionRepository.GetByIdAsync(id);
        if (submission == null) return null;

        return new SubmissionResponse
        {
            Id = submission.Id,
            UserId = submission.UserId,
            LessonId = submission.LessonId,
            Code = submission.Code,
            Passed = submission.Passed,
            SubmittedAt = submission.SubmittedAt,
            ExecutionTimeMs = submission.ExecutionTimeMs,
            ErrorMessage = submission.ErrorMessage,
            Username = submission.User?.Username ?? "",
            FirstName = submission.User?.FirstName,
            LastName = submission.User?.LastName,
            ProfilePictureUrl = submission.User?.ProfilePictureUrl,
            LessonTitle = submission.Lesson?.Title ?? "",
            LessonSlug = submission.Lesson?.Slug ?? ""
        };
    }

    public async Task<IEnumerable<SubmissionResponse>> GetUserSubmissionsAsync(Guid userId)
    {
        var submissions = await _submissionRepository.GetByUserIdAsync(userId);

        return submissions.Select(s => new SubmissionResponse
        {
            Id = s.Id,
            UserId = s.UserId,
            LessonId = s.LessonId,
            Code = s.Code,
            Passed = s.Passed,
            SubmittedAt = s.SubmittedAt,
            ExecutionTimeMs = s.ExecutionTimeMs,
            ErrorMessage = s.ErrorMessage,
            Username = s.User?.Username ?? "",
            FirstName = s.User?.FirstName,
            LastName = s.User?.LastName,
            ProfilePictureUrl = s.User?.ProfilePictureUrl,
            LessonTitle = s.Lesson?.Title ?? "",
            LessonSlug = s.Lesson?.Slug ?? ""
        });
    }

    public async Task<IEnumerable<PublicSubmissionResponse>> GetPublicSubmissionsForLessonAsync(string lessonId, int limit = 50)
    {
        var submissions = await _submissionRepository.GetPublicPassedSubmissionsAsync(lessonId, limit);

        return submissions.Select(s => new PublicSubmissionResponse
        {
            Id = s.Id,
            UserId = s.UserId,
            LessonId = s.LessonId,
            Code = s.Code,
            SubmittedAt = s.SubmittedAt,
            ExecutionTimeMs = s.ExecutionTimeMs,
            Username = s.User?.Username ?? "",
            FirstName = s.User?.FirstName,
            ProfilePictureUrl = s.User?.ProfilePictureUrl,
            LessonTitle = s.Lesson?.Title ?? "",
            LessonSlug = s.Lesson?.Slug ?? ""
        });
    }

    public async Task<IEnumerable<PublicSubmissionResponse>> GetUserPublicSubmissionsAsync(Guid userId, int limit = 50)
    {
        var submissions = await _submissionRepository.GetUserPublicPassedSubmissionsAsync(userId, limit);

        return submissions.Select(s => new PublicSubmissionResponse
        {
            Id = s.Id,
            UserId = s.UserId,
            LessonId = s.LessonId,
            Code = s.Code,
            SubmittedAt = s.SubmittedAt,
            ExecutionTimeMs = s.ExecutionTimeMs,
            Username = s.User?.Username ?? "",
            FirstName = s.User?.FirstName,
            ProfilePictureUrl = s.User?.ProfilePictureUrl,
            LessonTitle = s.Lesson?.Title ?? "",
            LessonSlug = s.Lesson?.Slug ?? ""
        });
    }

    public async Task<int> CreateSubmissionAsync(Submission submission)
    {
        return await _submissionRepository.CreateAsync(submission);
    }

    public async Task UpdateSubmissionAsync(Submission submission)
    {
        await _submissionRepository.UpdateAsync(submission);
    }

    public async Task DeleteSubmissionAsync(int id)
    {
        await _submissionRepository.DeleteAsync(id);
    }

    public async Task<Submission?> GetLatestSubmissionAsync(Guid userId, string lessonId)
    {
        return await _submissionRepository.GetLatestByUserAndLessonAsync(userId, lessonId);
    }

    public async Task<int> GetUserSubmissionCountAsync(Guid userId)
    {
        return await _submissionRepository.GetSubmissionCountByUserAsync(userId);
    }

    public async Task<int> GetUserPassedSubmissionCountAsync(Guid userId)
    {
        return await _submissionRepository.GetPassedSubmissionCountByUserAsync(userId);
    }

    public async Task<IEnumerable<PublicSubmissionResponse>> GetSubmissionsForReviewAsync(string lessonId, int limit = 50)
    {
        var submissions = await _submissionRepository.GetSubmissionsForReviewAsync(lessonId, limit);

        return submissions.Select(s => new PublicSubmissionResponse
        {
            Id = s.Id,
            UserId = s.UserId,
            LessonId = s.LessonId,
            Code = s.Code,
            SubmittedAt = s.SubmittedAt,
            ExecutionTimeMs = s.ExecutionTimeMs,
            Username = s.User?.Username ?? "",
            FirstName = s.User?.FirstName,
            ProfilePictureUrl = s.User?.ProfilePictureUrl,
            LessonTitle = s.Lesson?.Title ?? "",
            LessonSlug = s.Lesson?.Slug ?? ""
        });
    }

    public async Task<PublicSubmissionResponse?> GetSubmissionForReviewAsync(int submissionId)
    {
        var submission = await _submissionRepository.GetSubmissionForReviewAsync(submissionId);
        if (submission == null) return null;

        return new PublicSubmissionResponse
        {
            Id = submission.Id,
            UserId = submission.UserId,
            LessonId = submission.LessonId,
            Code = submission.Code,
            SubmittedAt = submission.SubmittedAt,
            ExecutionTimeMs = submission.ExecutionTimeMs,
            Username = submission.User?.Username ?? "",
            FirstName = submission.User?.FirstName,
            ProfilePictureUrl = submission.User?.ProfilePictureUrl,
            LessonTitle = submission.Lesson?.Title ?? "",
            LessonSlug = submission.Lesson?.Slug ?? ""
        };
    }
}