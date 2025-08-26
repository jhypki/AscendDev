using AscendDev.Core.DTOs.Auth;
using AscendDev.Core.DTOs.Social;
using AscendDev.Core.Exceptions;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.Social;
using Microsoft.Extensions.Logging;

namespace AscendDev.Services.Services;

public class CodeReviewService : ICodeReviewService
{
    private readonly ICodeReviewRepository _codeReviewRepository;
    private readonly ISubmissionRepository _submissionRepository;
    private readonly ISubmissionService _submissionService;
    private readonly ILogger<CodeReviewService> _logger;

    public CodeReviewService(
        ICodeReviewRepository codeReviewRepository,
        ISubmissionRepository submissionRepository,
        ISubmissionService submissionService,
        ILogger<CodeReviewService> logger)
    {
        _codeReviewRepository = codeReviewRepository;
        _submissionRepository = submissionRepository;
        _submissionService = submissionService;
        _logger = logger;
    }

    public async Task<CodeReviewResponse?> GetByIdAsync(Guid id)
    {
        try
        {
            var codeReview = await _codeReviewRepository.GetByIdAsync(id);
            return codeReview != null ? await MapToResponseAsync(codeReview) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting code review by id {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<CodeReviewResponse>> GetByLessonIdAsync(string lessonId, int page = 1, int pageSize = 20)
    {
        if (string.IsNullOrEmpty(lessonId))
            throw new BadRequestException("Lesson ID cannot be null or empty");

        try
        {
            var codeReviews = await _codeReviewRepository.GetByLessonIdAsync(lessonId, page, pageSize);
            var responses = new List<CodeReviewResponse>();

            foreach (var codeReview in codeReviews)
            {
                responses.Add(await MapToResponseAsync(codeReview));
            }

            return responses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting code reviews by lesson id {LessonId}", lessonId);
            throw;
        }
    }

    public async Task<IEnumerable<CodeReviewResponse>> GetByReviewerIdAsync(Guid reviewerId, int page = 1, int pageSize = 20)
    {
        try
        {
            var codeReviews = await _codeReviewRepository.GetByReviewerIdAsync(reviewerId, page, pageSize);
            var responses = new List<CodeReviewResponse>();

            foreach (var codeReview in codeReviews)
            {
                responses.Add(await MapToResponseAsync(codeReview));
            }

            return responses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting code reviews by reviewer id {ReviewerId}", reviewerId);
            throw;
        }
    }

    public async Task<IEnumerable<CodeReviewResponse>> GetByRevieweeIdAsync(Guid revieweeId, int page = 1, int pageSize = 20)
    {
        try
        {
            var codeReviews = await _codeReviewRepository.GetByRevieweeIdAsync(revieweeId, page, pageSize);
            var responses = new List<CodeReviewResponse>();

            foreach (var codeReview in codeReviews)
            {
                responses.Add(await MapToResponseAsync(codeReview));
            }

            return responses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting code reviews by reviewee id {RevieweeId}", revieweeId);
            throw;
        }
    }

    public async Task<IEnumerable<CodeReviewResponse>> GetByStatusAsync(CodeReviewStatus status, int page = 1, int pageSize = 20)
    {
        try
        {
            var codeReviews = await _codeReviewRepository.GetByStatusAsync(status, page, pageSize);
            var responses = new List<CodeReviewResponse>();

            foreach (var codeReview in codeReviews)
            {
                responses.Add(await MapToResponseAsync(codeReview));
            }

            return responses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting code reviews by status {Status}", status);
            throw;
        }
    }

    public async Task<CodeReviewResponse> CreateAsync(CreateCodeReviewRequest request, Guid reviewerId)
    {
        if (request == null)
            throw new BadRequestException("Code review request cannot be null");

        if (request.RevieweeId == reviewerId)
            throw new BadRequestException("You cannot review your own code");

        try
        {
            // Verify submission exists and belongs to the reviewee
            var submission = await _submissionRepository.GetByIdAsync(request.SubmissionId);
            if (submission == null)
                throw new NotFoundException("Submission", request.SubmissionId.ToString());

            if (submission.UserId != request.RevieweeId)
                throw new BadRequestException("Submission does not belong to the specified reviewee");

            if (submission.LessonId != request.LessonId)
                throw new BadRequestException("Submission lesson does not match the specified lesson");

            var codeReview = new CodeReview
            {
                Id = Guid.NewGuid(),
                LessonId = request.LessonId,
                ReviewerId = reviewerId,
                RevieweeId = request.RevieweeId,
                SubmissionId = request.SubmissionId,
                Status = CodeReviewStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            var createdCodeReview = await _codeReviewRepository.CreateAsync(codeReview);
            return await MapToResponseAsync(createdCodeReview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating code review for reviewer {ReviewerId}", reviewerId);
            throw;
        }
    }

    public async Task<CodeReviewResponse> UpdateAsync(Guid id, UpdateCodeReviewRequest request, Guid userId)
    {
        if (request == null)
            throw new BadRequestException("Update request cannot be null");

        try
        {
            var existingCodeReview = await _codeReviewRepository.GetByIdAsync(id);
            if (existingCodeReview == null)
                throw new NotFoundException("Code review", id.ToString());

            // Only reviewer or reviewee can update
            if (existingCodeReview.ReviewerId != userId && existingCodeReview.RevieweeId != userId)
                throw new UnauthorizedException("You can only update code reviews you are involved in");

            // Update only provided fields
            if (request.Status.HasValue)
            {
                existingCodeReview.Status = request.Status.Value;

                // Set completion date if status is completed or approved
                if (request.Status.Value == CodeReviewStatus.Completed ||
                    request.Status.Value == CodeReviewStatus.Approved)
                {
                    existingCodeReview.CompletedAt = DateTime.UtcNow;
                }
            }

            if (request.SubmissionId.HasValue)
            {
                // Only reviewee can update submission
                if (existingCodeReview.RevieweeId != userId)
                    throw new UnauthorizedException("Only the reviewee can update the submission");

                // Verify new submission exists and belongs to the reviewee
                var submission = await _submissionRepository.GetByIdAsync(request.SubmissionId.Value);
                if (submission == null)
                    throw new NotFoundException("Submission", request.SubmissionId.Value.ToString());

                if (submission.UserId != existingCodeReview.RevieweeId)
                    throw new BadRequestException("Submission does not belong to the reviewee");

                if (submission.LessonId != existingCodeReview.LessonId)
                    throw new BadRequestException("Submission lesson does not match the code review lesson");

                existingCodeReview.SubmissionId = request.SubmissionId.Value;
            }

            existingCodeReview.UpdatedAt = DateTime.UtcNow;

            var updatedCodeReview = await _codeReviewRepository.UpdateAsync(existingCodeReview);
            return await MapToResponseAsync(updatedCodeReview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating code review {Id} for user {UserId}", id, userId);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        try
        {
            var existingCodeReview = await _codeReviewRepository.GetByIdAsync(id);
            if (existingCodeReview == null)
                throw new NotFoundException("Code review", id.ToString());

            // Only reviewer or reviewee can delete
            if (existingCodeReview.ReviewerId != userId && existingCodeReview.RevieweeId != userId)
                throw new UnauthorizedException("You can only delete code reviews you are involved in");

            return await _codeReviewRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting code review {Id} for user {UserId}", id, userId);
            throw;
        }
    }

    public async Task<int> GetTotalCountByLessonIdAsync(string lessonId)
    {
        if (string.IsNullOrEmpty(lessonId))
            throw new BadRequestException("Lesson ID cannot be null or empty");

        try
        {
            return await _codeReviewRepository.GetTotalCountByLessonIdAsync(lessonId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total count by lesson id {LessonId}", lessonId);
            throw;
        }
    }

    public async Task<int> GetTotalCountByReviewerIdAsync(Guid reviewerId)
    {
        try
        {
            return await _codeReviewRepository.GetTotalCountByReviewerIdAsync(reviewerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total count by reviewer id {ReviewerId}", reviewerId);
            throw;
        }
    }

    public async Task<int> GetTotalCountByRevieweeIdAsync(Guid revieweeId)
    {
        try
        {
            return await _codeReviewRepository.GetTotalCountByRevieweeIdAsync(revieweeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total count by reviewee id {RevieweeId}", revieweeId);
            throw;
        }
    }

    public async Task<IEnumerable<CodeReviewResponse>> GetPendingReviewsAsync(int page = 1, int pageSize = 20)
    {
        try
        {
            var codeReviews = await _codeReviewRepository.GetPendingReviewsAsync(page, pageSize);
            var responses = new List<CodeReviewResponse>();

            foreach (var codeReview in codeReviews)
            {
                responses.Add(await MapToResponseAsync(codeReview));
            }

            return responses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending reviews");
            throw;
        }
    }

    private async Task<CodeReviewResponse> MapToResponseAsync(CodeReview codeReview)
    {
        var submissionResponse = await _submissionService.GetSubmissionByIdAsync(codeReview.SubmissionId);

        return new CodeReviewResponse
        {
            Id = codeReview.Id,
            LessonId = codeReview.LessonId,
            ReviewerId = codeReview.ReviewerId,
            RevieweeId = codeReview.RevieweeId,
            SubmissionId = codeReview.SubmissionId,
            Submission = submissionResponse,
            Status = codeReview.Status,
            CreatedAt = codeReview.CreatedAt,
            UpdatedAt = codeReview.UpdatedAt,
            CompletedAt = codeReview.CompletedAt,
            IsCompleted = codeReview.IsCompleted,
            CommentCount = codeReview.CommentCount,
            ReviewDuration = codeReview.ReviewDuration,
            Reviewer = new UserDto
            {
                Id = codeReview.Reviewer.Id,
                Username = codeReview.Reviewer.Username,
                Email = codeReview.Reviewer.Email
            },
            Reviewee = new UserDto
            {
                Id = codeReview.Reviewee.Id,
                Username = codeReview.Reviewee.Username,
                Email = codeReview.Reviewee.Email
            }
        };
    }
}