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
    private readonly IDiscussionLikeRepository _discussionLikeRepository;
    private readonly IDiscussionReplyService _discussionReplyService;
    private readonly ILogger<DiscussionService> _logger;

    public DiscussionService(
        IDiscussionRepository discussionRepository,
        IDiscussionLikeRepository discussionLikeRepository,
        IDiscussionReplyService discussionReplyService,
        ILogger<DiscussionService> logger)
    {
        _discussionRepository = discussionRepository;
        _discussionLikeRepository = discussionLikeRepository;
        _discussionReplyService = discussionReplyService;
        _logger = logger;
    }

    public async Task<DiscussionResponse?> GetByIdAsync(Guid id, Guid? currentUserId = null)
    {
        try
        {
            var discussion = await _discussionRepository.GetByIdAsync(id);
            if (discussion == null) return null;

            var response = await MapToResponseAsync(discussion, currentUserId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting discussion by id {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<DiscussionResponse>> GetByLessonIdAsync(string lessonId, int page = 1, int pageSize = 20, Guid? currentUserId = null)
    {
        if (string.IsNullOrEmpty(lessonId))
            throw new BadRequestException("Lesson ID cannot be null or empty");

        try
        {
            var discussions = await _discussionRepository.GetByLessonIdAsync(lessonId, page, pageSize);
            var responses = new List<DiscussionResponse>();

            foreach (var discussion in discussions)
            {
                var response = await MapToResponseAsync(discussion, currentUserId);
                responses.Add(response);
            }

            return responses;
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

        // Validate that either LessonId or CourseId is provided, but not both
        if (string.IsNullOrEmpty(request.LessonId) && string.IsNullOrEmpty(request.CourseId))
            throw new BadRequestException("Either LessonId or CourseId must be provided");

        if (!string.IsNullOrEmpty(request.LessonId) && !string.IsNullOrEmpty(request.CourseId))
            throw new BadRequestException("Cannot provide both LessonId and CourseId");

        try
        {
            var discussion = new Discussion
            {
                Id = Guid.NewGuid(),
                LessonId = request.LessonId,
                CourseId = request.CourseId,
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

    // Course-level discussion methods
    public async Task<IEnumerable<DiscussionResponse>> GetByCourseIdAsync(string courseId, int page = 1, int pageSize = 20, Guid? currentUserId = null)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        try
        {
            var discussions = await _discussionRepository.GetByCourseIdAsync(courseId, page, pageSize);
            var responses = new List<DiscussionResponse>();

            foreach (var discussion in discussions)
            {
                var response = await MapToResponseAsync(discussion, currentUserId);
                responses.Add(response);
            }

            return responses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting discussions by course id {CourseId}", courseId);
            throw;
        }
    }

    public async Task<int> GetTotalCountByCourseIdAsync(string courseId)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        try
        {
            return await _discussionRepository.GetTotalCountByCourseIdAsync(courseId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total count by course id {CourseId}", courseId);
            throw;
        }
    }

    public async Task<IEnumerable<DiscussionResponse>> GetPinnedByCourseIdAsync(string courseId)
    {
        if (string.IsNullOrEmpty(courseId))
            throw new BadRequestException("Course ID cannot be null or empty");

        try
        {
            var discussions = await _discussionRepository.GetPinnedByCourseIdAsync(courseId);
            return discussions.Select(MapToResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pinned discussions by course id {CourseId}", courseId);
            throw;
        }
    }

    // Like/Unlike methods
    public async Task<bool> LikeDiscussionAsync(Guid discussionId, Guid userId)
    {
        try
        {
            // Check if discussion exists
            var discussion = await _discussionRepository.GetByIdAsync(discussionId);
            if (discussion == null)
                throw new NotFoundException("Discussion", discussionId.ToString());

            // Check if already liked
            var existingLike = await _discussionLikeRepository.GetByDiscussionAndUserAsync(discussionId, userId);
            if (existingLike != null)
                return false; // Already liked

            // Create new like
            var like = new DiscussionLike
            {
                Id = Guid.NewGuid(),
                DiscussionId = discussionId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _discussionLikeRepository.CreateAsync(like);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error liking discussion {DiscussionId} for user {UserId}", discussionId, userId);
            throw;
        }
    }

    public async Task<bool> UnlikeDiscussionAsync(Guid discussionId, Guid userId)
    {
        try
        {
            return await _discussionLikeRepository.DeleteAsync(discussionId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unliking discussion {DiscussionId} for user {UserId}", discussionId, userId);
            throw;
        }
    }

    public async Task<int> GetLikeCountAsync(Guid discussionId)
    {
        try
        {
            return await _discussionLikeRepository.GetLikeCountAsync(discussionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting like count for discussion {DiscussionId}", discussionId);
            throw;
        }
    }

    public async Task<bool> IsLikedByUserAsync(Guid discussionId, Guid userId)
    {
        try
        {
            return await _discussionLikeRepository.IsLikedByUserAsync(discussionId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if discussion {DiscussionId} is liked by user {UserId}", discussionId, userId);
            throw;
        }
    }

    private async Task<DiscussionResponse> MapToResponseAsync(Discussion discussion, Guid? currentUserId = null)
    {
        // Get like count and status
        var likeCount = await _discussionLikeRepository.GetLikeCountAsync(discussion.Id);
        var isLikedByCurrentUser = currentUserId.HasValue
            ? await _discussionLikeRepository.IsLikedByUserAsync(discussion.Id, currentUserId.Value)
            : false;

        // Get replies
        var replies = await _discussionReplyService.GetByDiscussionIdAsync(discussion.Id);

        return new DiscussionResponse
        {
            Id = discussion.Id,
            LessonId = discussion.LessonId,
            CourseId = discussion.CourseId,
            UserId = discussion.UserId,
            Title = discussion.Title,
            Content = discussion.Content,
            CreatedAt = discussion.CreatedAt,
            UpdatedAt = discussion.UpdatedAt,
            IsPinned = discussion.IsPinned,
            IsLocked = discussion.IsLocked,
            ViewCount = discussion.ViewCount,
            ReplyCount = replies.Count(),
            LikeCount = likeCount,
            IsLikedByCurrentUser = isLikedByCurrentUser,
            LastActivity = discussion.LastActivity,
            User = new UserDto
            {
                Id = discussion.User.Id,
                Username = discussion.User.Username,
                Email = discussion.User.Email,
                FirstName = discussion.User.FirstName,
                LastName = discussion.User.LastName,
                ProfilePictureUrl = discussion.User.ProfilePictureUrl
            },
            Replies = replies.ToList()
        };
    }

    private static DiscussionResponse MapToResponse(Discussion discussion)
    {
        return new DiscussionResponse
        {
            Id = discussion.Id,
            LessonId = discussion.LessonId,
            CourseId = discussion.CourseId,
            UserId = discussion.UserId,
            Title = discussion.Title,
            Content = discussion.Content,
            CreatedAt = discussion.CreatedAt,
            UpdatedAt = discussion.UpdatedAt,
            IsPinned = discussion.IsPinned,
            IsLocked = discussion.IsLocked,
            ViewCount = discussion.ViewCount,
            ReplyCount = discussion.ReplyCount,
            LikeCount = discussion.LikeCount,
            IsLikedByCurrentUser = false,
            LastActivity = discussion.LastActivity,
            User = new UserDto
            {
                Id = discussion.User.Id,
                Username = discussion.User.Username,
                Email = discussion.User.Email,
                FirstName = discussion.User.FirstName,
                LastName = discussion.User.LastName,
                ProfilePictureUrl = discussion.User.ProfilePictureUrl
            }
        };
    }
}