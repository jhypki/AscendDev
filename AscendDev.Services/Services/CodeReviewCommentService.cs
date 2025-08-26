using AscendDev.Core.DTOs.Auth;
using AscendDev.Core.DTOs.Social;
using AscendDev.Core.Exceptions;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.Social;
using Microsoft.Extensions.Logging;

namespace AscendDev.Services.Services;

public class CodeReviewCommentService : ICodeReviewCommentService
{
    private readonly ICodeReviewCommentRepository _codeReviewCommentRepository;
    private readonly ICodeReviewRepository _codeReviewRepository;
    private readonly ILogger<CodeReviewCommentService> _logger;

    public CodeReviewCommentService(
        ICodeReviewCommentRepository codeReviewCommentRepository,
        ICodeReviewRepository codeReviewRepository,
        ILogger<CodeReviewCommentService> logger)
    {
        _codeReviewCommentRepository = codeReviewCommentRepository;
        _codeReviewRepository = codeReviewRepository;
        _logger = logger;
    }

    public async Task<CodeReviewCommentResponse?> GetByIdAsync(Guid id)
    {
        try
        {
            var comment = await _codeReviewCommentRepository.GetByIdAsync(id);
            return comment != null ? MapToResponse(comment) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting code review comment by id {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<CodeReviewCommentResponse>> GetByCodeReviewIdAsync(Guid codeReviewId)
    {
        try
        {
            var comments = await _codeReviewCommentRepository.GetByCodeReviewIdAsync(codeReviewId);
            return comments.Select(MapToResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting code review comments by code review id {CodeReviewId}", codeReviewId);
            throw;
        }
    }

    public async Task<IEnumerable<CodeReviewCommentResponse>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        try
        {
            var comments = await _codeReviewCommentRepository.GetByUserIdAsync(userId, page, pageSize);
            return comments.Select(MapToResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting code review comments by user id {UserId}", userId);
            throw;
        }
    }

    public async Task<CodeReviewCommentResponse> CreateAsync(Guid codeReviewId, CreateCodeReviewCommentRequest request, Guid userId)
    {
        if (request == null)
            throw new BadRequestException("Code review comment request cannot be null");

        try
        {
            // Verify code review exists and user has access
            var codeReview = await _codeReviewRepository.GetByIdAsync(codeReviewId);
            if (codeReview == null)
                throw new NotFoundException("Code review", codeReviewId.ToString());

            // Only reviewer and reviewee can comment
            if (codeReview.ReviewerId != userId && codeReview.RevieweeId != userId)
                throw new UnauthorizedException("You can only comment on code reviews you are involved in");

            var comment = new CodeReviewComment
            {
                Id = Guid.NewGuid(),
                CodeReviewId = codeReviewId,
                UserId = userId,
                LineNumber = request.LineNumber,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                IsResolved = false
            };

            var createdComment = await _codeReviewCommentRepository.CreateAsync(comment);
            return MapToResponse(createdComment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating code review comment for user {UserId}", userId);
            throw;
        }
    }

    public async Task<CodeReviewCommentResponse> UpdateAsync(Guid id, UpdateCodeReviewCommentRequest request, Guid userId)
    {
        if (request == null)
            throw new BadRequestException("Update request cannot be null");

        try
        {
            var existingComment = await _codeReviewCommentRepository.GetByIdAsync(id);
            if (existingComment == null)
                throw new NotFoundException("Code review comment", id.ToString());

            if (existingComment.UserId != userId)
                throw new UnauthorizedException("You can only update your own comments");

            // Update only provided fields
            if (!string.IsNullOrEmpty(request.Content))
                existingComment.Content = request.Content;

            if (request.IsResolved.HasValue)
                existingComment.IsResolved = request.IsResolved.Value;

            existingComment.UpdatedAt = DateTime.UtcNow;

            var updatedComment = await _codeReviewCommentRepository.UpdateAsync(existingComment);
            return MapToResponse(updatedComment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating code review comment {Id} for user {UserId}", id, userId);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        try
        {
            var existingComment = await _codeReviewCommentRepository.GetByIdAsync(id);
            if (existingComment == null)
                throw new NotFoundException("Code review comment", id.ToString());

            if (existingComment.UserId != userId)
                throw new UnauthorizedException("You can only delete your own comments");

            return await _codeReviewCommentRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting code review comment {Id} for user {UserId}", id, userId);
            throw;
        }
    }

    public async Task<IEnumerable<CodeReviewCommentResponse>> GetByLineNumberAsync(Guid codeReviewId, int lineNumber)
    {
        try
        {
            var comments = await _codeReviewCommentRepository.GetByLineNumberAsync(codeReviewId, lineNumber);
            return comments.Select(MapToResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comments by line number {LineNumber} for code review {CodeReviewId}", lineNumber, codeReviewId);
            throw;
        }
    }

    public async Task<IEnumerable<CodeReviewCommentResponse>> GetUnresolvedByCodeReviewIdAsync(Guid codeReviewId)
    {
        try
        {
            var comments = await _codeReviewCommentRepository.GetUnresolvedByCodeReviewIdAsync(codeReviewId);
            return comments.Select(MapToResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unresolved comments for code review {CodeReviewId}", codeReviewId);
            throw;
        }
    }

    public async Task<int> GetTotalCountByCodeReviewIdAsync(Guid codeReviewId)
    {
        try
        {
            return await _codeReviewCommentRepository.GetTotalCountByCodeReviewIdAsync(codeReviewId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total count by code review id {CodeReviewId}", codeReviewId);
            throw;
        }
    }

    private static CodeReviewCommentResponse MapToResponse(CodeReviewComment comment)
    {
        return new CodeReviewCommentResponse
        {
            Id = comment.Id,
            CodeReviewId = comment.CodeReviewId,
            UserId = comment.UserId,
            LineNumber = comment.LineNumber,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            IsResolved = comment.IsResolved,
            IsEdited = comment.IsEdited,
            IsLineComment = comment.IsLineComment,
            IsGeneralComment = comment.IsGeneralComment,
            User = new UserDto
            {
                Id = comment.User.Id,
                Username = comment.User.Username,
                Email = comment.User.Email
            }
        };
    }
}