using AscendDev.Core.DTOs;
using AscendDev.Core.DTOs.Social;
using AscendDev.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AscendDev.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class DiscussionsController : ControllerBase
{
    private readonly IDiscussionService _discussionService;
    private readonly IDiscussionReplyService _discussionReplyService;
    private readonly ILogger<DiscussionsController> _logger;

    public DiscussionsController(
        IDiscussionService discussionService,
        IDiscussionReplyService discussionReplyService,
        ILogger<DiscussionsController> logger)
    {
        _discussionService = discussionService;
        _discussionReplyService = discussionReplyService;
        _logger = logger;
    }

    /// <summary>
    /// Get discussion by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<DiscussionResponse>>> GetById(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var discussion = await _discussionService.GetByIdAsync(id, userId);
            if (discussion == null)
                return NotFound(new ErrorApiResponse(null, "Discussion not found"));

            // Increment view count
            await _discussionService.IncrementViewCountAsync(id);

            return Ok(new ApiResponse<DiscussionResponse>(true, discussion, "Success", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting discussion {Id}", id);
            return StatusCode(500, new ErrorApiResponse(null, "Internal server error"));
        }
    }

    /// <summary>
    /// Get discussions by lesson ID
    /// </summary>
    [HttpGet("lesson/{lessonId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DiscussionResponse>>>> GetByLessonId(
        string lessonId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            var discussions = await _discussionService.GetByLessonIdAsync(lessonId, page, pageSize, userId);
            return Ok(new ApiResponse<IEnumerable<DiscussionResponse>>(true, discussions, "Success", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting discussions for lesson {LessonId}", lessonId);
            return StatusCode(500, new ErrorApiResponse(null, "Internal server error"));
        }
    }

    /// <summary>
    /// Get pinned discussions by lesson ID
    /// </summary>
    [HttpGet("lesson/{lessonId}/pinned")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DiscussionResponse>>>> GetPinnedByLessonId(string lessonId)
    {
        try
        {
            var discussions = await _discussionService.GetPinnedByLessonIdAsync(lessonId);
            return Ok(new ApiResponse<IEnumerable<DiscussionResponse>>(true, discussions, "Success", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pinned discussions for lesson {LessonId}", lessonId);
            return StatusCode(500, new ErrorApiResponse(null, "Internal server error"));
        }
    }

    /// <summary>
    /// Get discussions by user ID
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DiscussionResponse>>>> GetByUserId(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var discussions = await _discussionService.GetByUserIdAsync(userId, page, pageSize);
            return Ok(new ApiResponse<IEnumerable<DiscussionResponse>>(true, discussions, "Success", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting discussions for user {UserId}", userId);
            return StatusCode(500, new ErrorApiResponse(null, "Internal server error"));
        }
    }

    /// <summary>
    /// Search discussions
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DiscussionResponse>>>> Search(
        [FromQuery] string searchTerm,
        [FromQuery] string? lessonId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var discussions = await _discussionService.SearchAsync(searchTerm, lessonId, page, pageSize);
            return Ok(new ApiResponse<IEnumerable<DiscussionResponse>>(true, discussions, "Success", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching discussions with term {SearchTerm}", searchTerm);
            return StatusCode(500, new ErrorApiResponse(null, "Internal server error"));
        }
    }

    /// <summary>
    /// Create a new discussion
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<DiscussionResponse>>> Create([FromBody] CreateDiscussionRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var discussion = await _discussionService.CreateAsync(request, userId);
            return CreatedAtAction(nameof(GetById), new { id = discussion.Id }, new ApiResponse<DiscussionResponse>(true, discussion, "Discussion created successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating discussion");
            return StatusCode(500, new ErrorApiResponse(null, "Internal server error"));
        }
    }

    /// <summary>
    /// Update a discussion
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<DiscussionResponse>>> Update(Guid id, [FromBody] UpdateDiscussionRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var discussion = await _discussionService.UpdateAsync(id, request, userId);
            return Ok(new ApiResponse<DiscussionResponse>(true, discussion, "Discussion updated successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating discussion {Id}", id);
            return StatusCode(500, new ErrorApiResponse(null, "Internal server error"));
        }
    }

    /// <summary>
    /// Delete a discussion
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _discussionService.DeleteAsync(id, userId);
            return Ok(new ApiResponse<bool>(true, result, "Discussion deleted successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting discussion {Id}", id);
            return StatusCode(500, new ErrorApiResponse(null, "Internal server error"));
        }
    }

    /// <summary>
    /// Get total count of discussions by lesson ID
    /// </summary>
    [HttpGet("lesson/{lessonId}/count")]
    public async Task<ActionResult<ApiResponse<int>>> GetTotalCountByLessonId(string lessonId)
    {
        try
        {
            var count = await _discussionService.GetTotalCountByLessonIdAsync(lessonId);
            return Ok(new ApiResponse<int>(true, count, "Success", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting discussion count for lesson {LessonId}", lessonId);
            return StatusCode(500, new ErrorApiResponse(null, "Internal server error"));
        }
    }

    /// <summary>
    /// Like a discussion
    /// </summary>
    [HttpPost("{id}/like")]
    public async Task<ActionResult<ApiResponse<bool>>> LikeDiscussion(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _discussionService.LikeDiscussionAsync(id, userId);
            return Ok(new ApiResponse<bool>(true, result, "Discussion liked successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error liking discussion {Id}", id);
            return StatusCode(500, new ErrorApiResponse(null, "Internal server error"));
        }
    }

    /// <summary>
    /// Unlike a discussion
    /// </summary>
    [HttpDelete("{id}/like")]
    public async Task<ActionResult<ApiResponse<bool>>> UnlikeDiscussion(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _discussionService.UnlikeDiscussionAsync(id, userId);
            return Ok(new ApiResponse<bool>(true, result, "Discussion unliked successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unliking discussion {Id}", id);
            return StatusCode(500, new ErrorApiResponse(null, "Internal server error"));
        }
    }

    /// <summary>
    /// Get like count for a discussion
    /// </summary>
    [HttpGet("{id}/likes/count")]
    public async Task<ActionResult<ApiResponse<int>>> GetLikeCount(Guid id)
    {
        try
        {
            var count = await _discussionService.GetLikeCountAsync(id);
            return Ok(new ApiResponse<int>(true, count, "Success", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting like count for discussion {Id}", id);
            return StatusCode(500, new ErrorApiResponse(null, "Internal server error"));
        }
    }

    /// <summary>
    /// Check if discussion is liked by current user
    /// </summary>
    [HttpGet("{id}/likes/status")]
    public async Task<ActionResult<ApiResponse<bool>>> GetLikeStatus(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var isLiked = await _discussionService.IsLikedByUserAsync(id, userId);
            return Ok(new ApiResponse<bool>(true, isLiked, "Success", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting like status for discussion {Id}", id);
            return StatusCode(500, new ErrorApiResponse(null, "Internal server error"));
        }
    }

    /// <summary>
    /// Get replies for a discussion
    /// </summary>
    [HttpGet("{id}/replies")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DiscussionReplyResponse>>>> GetReplies(Guid id)
    {
        try
        {
            var replies = await _discussionReplyService.GetByDiscussionIdAsync(id);
            return Ok(new ApiResponse<IEnumerable<DiscussionReplyResponse>>(true, replies, "Success", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting replies for discussion {Id}", id);
            return StatusCode(500, new ErrorApiResponse(null, "Internal server error"));
        }
    }

    /// <summary>
    /// Create a reply to a discussion
    /// </summary>
    [HttpPost("{id}/replies")]
    public async Task<ActionResult<ApiResponse<DiscussionReplyResponse>>> CreateReply(Guid id, [FromBody] CreateDiscussionReplyRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var reply = await _discussionReplyService.CreateAsync(id, request, userId);
            return CreatedAtAction(nameof(GetReplies), new { id }, new ApiResponse<DiscussionReplyResponse>(true, reply, "Reply created successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reply for discussion {Id}", id);
            return StatusCode(500, new ErrorApiResponse(null, "Internal server error"));
        }
    }

    /// <summary>
    /// Update a reply
    /// </summary>
    [HttpPut("replies/{replyId}")]
    public async Task<ActionResult<ApiResponse<DiscussionReplyResponse>>> UpdateReply(Guid replyId, [FromBody] UpdateDiscussionReplyRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var reply = await _discussionReplyService.UpdateAsync(replyId, request, userId);
            return Ok(new ApiResponse<DiscussionReplyResponse>(true, reply, "Reply updated successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating reply {ReplyId}", replyId);
            return StatusCode(500, new ErrorApiResponse(null, "Internal server error"));
        }
    }

    /// <summary>
    /// Delete a reply
    /// </summary>
    [HttpDelete("replies/{replyId}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteReply(Guid replyId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _discussionReplyService.DeleteAsync(replyId, userId);
            return Ok(new ApiResponse<bool>(true, result, "Reply deleted successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting reply {ReplyId}", replyId);
            return StatusCode(500, new ErrorApiResponse(null, "Internal server error"));
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token");

        return userId;
    }
}