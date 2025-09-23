using AscendDev.Core.DTOs;
using AscendDev.Core.DTOs.Social;
using AscendDev.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AscendDev.API.Controllers;

[ApiController]
[Route("api/courses/{courseId}/[controller]")]
[Authorize]
public class CourseDiscussionsController : ControllerBase
{
    private readonly IDiscussionService _discussionService;
    private readonly ILogger<CourseDiscussionsController> _logger;

    public CourseDiscussionsController(
        IDiscussionService discussionService,
        ILogger<CourseDiscussionsController> logger)
    {
        _discussionService = discussionService;
        _logger = logger;
    }

    /// <summary>
    /// Get discussions for a specific course
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<DiscussionResponse>>>> GetCourseDiscussions(
        string courseId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            var discussions = await _discussionService.GetByCourseIdAsync(courseId, page, pageSize, userId);
            return Ok(new ApiResponse<IEnumerable<DiscussionResponse>>(true, discussions, "Success", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting discussions for course {CourseId}", courseId);
            return StatusCode(500, new ErrorApiResponse(null, "Internal server error"));
        }
    }

    /// <summary>
    /// Create a new discussion for a course
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<DiscussionResponse>>> CreateCourseDiscussion(
        string courseId,
        [FromBody] CreateDiscussionRequest request)
    {
        try
        {
            // Override the CourseId from the route
            request.CourseId = courseId;
            request.LessonId = null; // Ensure lesson ID is null for course discussions

            var userId = GetCurrentUserId();
            var discussion = await _discussionService.CreateAsync(request, userId);
            return CreatedAtAction(nameof(GetCourseDiscussion), new { courseId, id = discussion.Id },
                new ApiResponse<DiscussionResponse>(true, discussion, "Discussion created successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating discussion for course {CourseId}", courseId);
            return StatusCode(500, new ErrorApiResponse(null, "Internal server error"));
        }
    }

    /// <summary>
    /// Get a specific discussion by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<DiscussionResponse>>> GetCourseDiscussion(string courseId, Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var discussion = await _discussionService.GetByIdAsync(id, userId);
            if (discussion == null)
                return NotFound(new ErrorApiResponse(null, "Discussion not found"));

            // Verify the discussion belongs to the specified course
            if (discussion.CourseId != courseId)
                return NotFound(new ErrorApiResponse(null, "Discussion not found"));

            // Increment view count
            await _discussionService.IncrementViewCountAsync(id);

            return Ok(new ApiResponse<DiscussionResponse>(true, discussion, "Success", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting discussion {Id} for course {CourseId}", id, courseId);
            return StatusCode(500, new ErrorApiResponse(null, "Internal server error"));
        }
    }

    /// <summary>
    /// Update a discussion
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<DiscussionResponse>>> UpdateCourseDiscussion(
        string courseId,
        Guid id,
        [FromBody] UpdateDiscussionRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var discussion = await _discussionService.UpdateAsync(id, request, userId);

            // Verify the discussion belongs to the specified course
            if (discussion.CourseId != courseId)
                return NotFound(new ErrorApiResponse(null, "Discussion not found"));

            return Ok(new ApiResponse<DiscussionResponse>(true, discussion, "Discussion updated successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating discussion {Id} for course {CourseId}", id, courseId);
            return StatusCode(500, new ErrorApiResponse(null, "Internal server error"));
        }
    }

    /// <summary>
    /// Delete a discussion
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCourseDiscussion(string courseId, Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _discussionService.DeleteAsync(id, userId);
            return Ok(new ApiResponse<bool>(true, result, "Discussion deleted successfully", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting discussion {Id} for course {CourseId}", id, courseId);
            return StatusCode(500, new ErrorApiResponse(null, "Internal server error"));
        }
    }

    /// <summary>
    /// Get total count of discussions for a course
    /// </summary>
    [HttpGet("count")]
    public async Task<ActionResult<ApiResponse<int>>> GetCourseDiscussionCount(string courseId)
    {
        try
        {
            var count = await _discussionService.GetTotalCountByCourseIdAsync(courseId);
            return Ok(new ApiResponse<int>(true, count, "Success", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting discussion count for course {CourseId}", courseId);
            return StatusCode(500, new ErrorApiResponse(null, "Internal server error"));
        }
    }

    /// <summary>
    /// Create a reply to a course discussion
    /// </summary>
    [HttpPost("{discussionId}/replies")]
    public async Task<ActionResult<ApiResponse<DiscussionReplyResponse>>> CreateCourseDiscussionReply(
        string courseId,
        Guid discussionId,
        [FromBody] CreateDiscussionReplyRequest request)
    {
        try
        {
            // This would need to be implemented in the DiscussionReplyService
            // For now, return a placeholder response
            return StatusCode(501, new ErrorApiResponse(null, "Reply functionality not yet implemented"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reply for discussion {DiscussionId} in course {CourseId}", discussionId, courseId);
            return StatusCode(500, new ErrorApiResponse(null, "Internal server error"));
        }
    }

    /// <summary>
    /// Update a discussion reply
    /// </summary>
    [HttpPut("replies/{replyId}")]
    public async Task<ActionResult<ApiResponse<DiscussionReplyResponse>>> UpdateCourseDiscussionReply(
        string courseId,
        Guid replyId,
        [FromBody] UpdateDiscussionReplyRequest request)
    {
        try
        {
            // This would need to be implemented in the DiscussionReplyService
            // For now, return a placeholder response
            return StatusCode(501, new ErrorApiResponse(null, "Reply functionality not yet implemented"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating reply {ReplyId} in course {CourseId}", replyId, courseId);
            return StatusCode(500, new ErrorApiResponse(null, "Internal server error"));
        }
    }

    /// <summary>
    /// Delete a discussion reply
    /// </summary>
    [HttpDelete("replies/{replyId}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCourseDiscussionReply(string courseId, Guid replyId)
    {
        try
        {
            // This would need to be implemented in the DiscussionReplyService
            // For now, return a placeholder response
            return StatusCode(501, new ErrorApiResponse(null, "Reply functionality not yet implemented"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting reply {ReplyId} in course {CourseId}", replyId, courseId);
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