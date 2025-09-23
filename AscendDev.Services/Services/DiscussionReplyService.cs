using AscendDev.Core.DTOs.Auth;
using AscendDev.Core.DTOs.Social;
using AscendDev.Core.Exceptions;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.Social;
using Microsoft.Extensions.Logging;

namespace AscendDev.Services.Services;

public class DiscussionReplyService : IDiscussionReplyService
{
    private readonly IDiscussionReplyRepository _discussionReplyRepository;
    private readonly IDiscussionRepository _discussionRepository;
    private readonly ILogger<DiscussionReplyService> _logger;

    public DiscussionReplyService(
        IDiscussionReplyRepository discussionReplyRepository,
        IDiscussionRepository discussionRepository,
        ILogger<DiscussionReplyService> logger)
    {
        _discussionReplyRepository = discussionReplyRepository;
        _discussionRepository = discussionRepository;
        _logger = logger;
    }

    public async Task<DiscussionReplyResponse?> GetByIdAsync(Guid id)
    {
        try
        {
            var reply = await _discussionReplyRepository.GetByIdAsync(id);
            return reply != null ? MapToResponse(reply) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting discussion reply by id {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<DiscussionReplyResponse>> GetByDiscussionIdAsync(Guid discussionId)
    {
        try
        {
            var replies = await _discussionReplyRepository.GetByDiscussionIdAsync(discussionId);
            return replies.Select(MapToResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting discussion replies by discussion id {DiscussionId}", discussionId);
            throw;
        }
    }

    public async Task<IEnumerable<DiscussionReplyResponse>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        try
        {
            var replies = await _discussionReplyRepository.GetByUserIdAsync(userId, page, pageSize);
            return replies.Select(MapToResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting discussion replies by user id {UserId}", userId);
            throw;
        }
    }

    public async Task<DiscussionReplyResponse> CreateAsync(Guid discussionId, CreateDiscussionReplyRequest request, Guid userId)
    {
        if (request == null)
            throw new BadRequestException("Discussion reply request cannot be null");

        try
        {
            // Verify discussion exists
            var discussion = await _discussionRepository.GetByIdAsync(discussionId);
            if (discussion == null)
                throw new NotFoundException("Discussion", discussionId.ToString());

            if (discussion.IsLocked)
                throw new BadRequestException("Cannot reply to a locked discussion");

            // Verify parent reply exists if specified
            if (request.ParentReplyId.HasValue)
            {
                var parentReply = await _discussionReplyRepository.GetByIdAsync(request.ParentReplyId.Value);
                if (parentReply == null)
                    throw new NotFoundException("Parent reply", request.ParentReplyId.Value.ToString());

                if (parentReply.DiscussionId != discussionId)
                    throw new BadRequestException("Parent reply does not belong to this discussion");
            }

            var reply = new DiscussionReply
            {
                Id = Guid.NewGuid(),
                DiscussionId = discussionId,
                UserId = userId,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                ParentReplyId = request.ParentReplyId,
                IsSolution = false
            };

            var createdReply = await _discussionReplyRepository.CreateAsync(reply);
            return MapToResponse(createdReply);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating discussion reply for user {UserId}", userId);
            throw;
        }
    }

    public async Task<DiscussionReplyResponse> UpdateAsync(Guid id, UpdateDiscussionReplyRequest request, Guid userId)
    {
        if (request == null)
            throw new BadRequestException("Update request cannot be null");

        try
        {
            var existingReply = await _discussionReplyRepository.GetByIdAsync(id);
            if (existingReply == null)
                throw new NotFoundException("Discussion reply", id.ToString());

            if (existingReply.UserId != userId)
                throw new UnauthorizedException("You can only update your own replies");

            // Update only provided fields
            if (!string.IsNullOrEmpty(request.Content))
                existingReply.Content = request.Content;

            if (request.IsSolution.HasValue)
                existingReply.IsSolution = request.IsSolution.Value;

            existingReply.UpdatedAt = DateTime.UtcNow;

            var updatedReply = await _discussionReplyRepository.UpdateAsync(existingReply);
            return MapToResponse(updatedReply);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating discussion reply {Id} for user {UserId}", id, userId);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        try
        {
            var existingReply = await _discussionReplyRepository.GetByIdAsync(id);
            if (existingReply == null)
                throw new NotFoundException("Discussion reply", id.ToString());

            if (existingReply.UserId != userId)
                throw new UnauthorizedException("You can only delete your own replies");

            return await _discussionReplyRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting discussion reply {Id} for user {UserId}", id, userId);
            throw;
        }
    }

    public async Task<IEnumerable<DiscussionReplyResponse>> GetChildRepliesAsync(Guid parentReplyId)
    {
        try
        {
            var replies = await _discussionReplyRepository.GetChildRepliesAsync(parentReplyId);
            return replies.Select(MapToResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting child replies for parent {ParentReplyId}", parentReplyId);
            throw;
        }
    }

    public async Task<int> GetTotalCountByDiscussionIdAsync(Guid discussionId)
    {
        try
        {
            return await _discussionReplyRepository.GetTotalCountByDiscussionIdAsync(discussionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total count by discussion id {DiscussionId}", discussionId);
            throw;
        }
    }

    public async Task<DiscussionReplyResponse?> GetSolutionByDiscussionIdAsync(Guid discussionId)
    {
        try
        {
            var solution = await _discussionReplyRepository.GetSolutionByDiscussionIdAsync(discussionId);
            return solution != null ? MapToResponse(solution) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting solution by discussion id {DiscussionId}", discussionId);
            throw;
        }
    }

    private static DiscussionReplyResponse MapToResponse(DiscussionReply reply)
    {
        return new DiscussionReplyResponse
        {
            Id = reply.Id,
            DiscussionId = reply.DiscussionId,
            UserId = reply.UserId,
            Content = reply.Content,
            CreatedAt = reply.CreatedAt,
            UpdatedAt = reply.UpdatedAt,
            ParentReplyId = reply.ParentReplyId,
            IsSolution = reply.IsSolution,
            IsEdited = reply.IsEdited,
            Depth = reply.Depth,
            User = reply.User != null ? new UserDto
            {
                Id = reply.User.Id,
                Username = reply.User.Username,
                Email = reply.User.Email,
                FirstName = reply.User.FirstName,
                LastName = reply.User.LastName,
                ProfilePictureUrl = reply.User.ProfilePictureUrl
            } : null
        };
    }
}