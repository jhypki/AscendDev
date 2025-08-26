using AscendDev.Core.DTOs.Auth;
using AscendDev.Core.DTOs.Social;
using AscendDev.Core.Exceptions;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.Social;
using Microsoft.Extensions.Logging;

namespace AscendDev.Services.Services;

public class DiscussionService : IDiscussionService
{
    private readonly IDiscussionRepository _discussionRepository;
    private readonly ILogger<DiscussionService> _logger;

    public DiscussionService(
        IDiscussionRepository discussionRepository,
        ILogger<DiscussionService> logger)
    {
        _discussionRepository = discussionRepository;
        _logger = logger;
    }

    public async Task<DiscussionResponse?> GetByIdAsync(Guid id)
    {
        try
        {
            var discussion = await _discussionRepository.GetByIdAsync(id);
            return discussion != null ? MapToResponse(discussion) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting discussion by id {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<DiscussionResponse>> GetByLessonIdAsync(string lessonId, int page = 1, int pageSize = 20)
    {
        if (string.IsNullOrEmpty(lessonId))
            throw new BadRequestException("Lesson ID cannot be null or empty");

        try
        {
            var discussions = await _discussionRepository.GetByLessonIdAsync(lessonId, page, pageSize);
            return discussions.Select(MapToResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting discussions by lesson id {LessonId}", lessonId);
            throw;
        }
    }

    public async Task<IEnumerable<DiscussionResponse>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        try
        {
            var discussions = await _discussionRepository.GetByUserIdAsync(userId, page, pageSize);
            return discussions.Select(MapToResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting discussions by user id {UserId}", userId);
            throw;
        }
    }

    public async Task<DiscussionResponse> CreateAsync(CreateDiscussionRequest request, Guid userId)
    {
        if (request == null)
            throw new BadRequestException("Discussion request cannot be null");

        try
        {
            var discussion = new Discussion
            {
                Id = Guid.NewGuid(),
                LessonId = request.LessonId,
                UserId = userId,
                Title = request.Title,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                IsPinned = false,
                IsLocked = false,
                ViewCount = 0
            };

            var createdDiscussion = await _discussionRepository.CreateAsync(discussion);
            return MapToResponse(createdDiscussion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating discussion for user {UserId}", userId);
            throw;
        }
    }

    public async Task<DiscussionResponse> UpdateAsync(Guid id, UpdateDiscussionRequest request, Guid userId)
    {
        if (request == null)
            throw new BadRequestException("Update request cannot be null");

        try
        {
            var existingDiscussion = await _discussionRepository.GetByIdAsync(id);
            if (existingDiscussion == null)
                throw new NotFoundException("Discussion", id.ToString());

            if (existingDiscussion.UserId != userId)
                throw new UnauthorizedException("You can only update your own discussions");

            // Update only provided fields
            if (!string.IsNullOrEmpty(request.Title))
                existingDiscussion.Title = request.Title;

            if (!string.IsNullOrEmpty(request.Content))
                existingDiscussion.Content = request.Content;

            if (request.IsPinned.HasValue)
                existingDiscussion.IsPinned = request.IsPinned.Value;

            if (request.IsLocked.HasValue)
                existingDiscussion.IsLocked = request.IsLocked.Value;

            existingDiscussion.UpdatedAt = DateTime.UtcNow;

            var updatedDiscussion = await _discussionRepository.UpdateAsync(existingDiscussion);
            return MapToResponse(updatedDiscussion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating discussion {Id} for user {UserId}", id, userId);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        try
        {
            var existingDiscussion = await _discussionRepository.GetByIdAsync(id);
            if (existingDiscussion == null)
                throw new NotFoundException("Discussion", id.ToString());

            if (existingDiscussion.UserId != userId)
                throw new UnauthorizedException("You can only delete your own discussions");

            return await _discussionRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting discussion {Id} for user {UserId}", id, userId);
            throw;
        }
    }

    public async Task<bool> IncrementViewCountAsync(Guid id)
    {
        try
        {
            return await _discussionRepository.IncrementViewCountAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing view count for discussion {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<DiscussionResponse>> GetPinnedByLessonIdAsync(string lessonId)
    {
        if (string.IsNullOrEmpty(lessonId))
            throw new BadRequestException("Lesson ID cannot be null or empty");

        try
        {
            var discussions = await _discussionRepository.GetPinnedByLessonIdAsync(lessonId);
            return discussions.Select(MapToResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pinned discussions by lesson id {LessonId}", lessonId);
            throw;
        }
    }

    public async Task<int> GetTotalCountByLessonIdAsync(string lessonId)
    {
        if (string.IsNullOrEmpty(lessonId))
            throw new BadRequestException("Lesson ID cannot be null or empty");

        try
        {
            return await _discussionRepository.GetTotalCountByLessonIdAsync(lessonId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total count by lesson id {LessonId}", lessonId);
            throw;
        }
    }

    public async Task<IEnumerable<DiscussionResponse>> SearchAsync(string searchTerm, string? lessonId = null, int page = 1, int pageSize = 20)
    {
        if (string.IsNullOrEmpty(searchTerm))
            throw new BadRequestException("Search term cannot be null or empty");

        try
        {
            var discussions = await _discussionRepository.SearchAsync(searchTerm, lessonId, page, pageSize);
            return discussions.Select(MapToResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching discussions with term {SearchTerm}", searchTerm);
            throw;
        }
    }

    private static DiscussionResponse MapToResponse(Discussion discussion)
    {
        return new DiscussionResponse
        {
            Id = discussion.Id,
            LessonId = discussion.LessonId,
            UserId = discussion.UserId,
            Title = discussion.Title,
            Content = discussion.Content,
            CreatedAt = discussion.CreatedAt,
            UpdatedAt = discussion.UpdatedAt,
            IsPinned = discussion.IsPinned,
            IsLocked = discussion.IsLocked,
            ViewCount = discussion.ViewCount,
            ReplyCount = discussion.ReplyCount,
            LastActivity = discussion.LastActivity,
            User = new UserDto
            {
                Id = discussion.User.Id,
                Username = discussion.User.Username,
                Email = discussion.User.Email
            }
        };
    }
}