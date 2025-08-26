using AscendDev.Core.DTOs;
using AscendDev.Core.DTOs.Social;
using AscendDev.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AscendDev.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DiscussionsController : ControllerBase
{
    private readonly IDiscussionService _discussionService;
    private readonly ILogger<DiscussionsController> _logger;

    public DiscussionsController(
        IDiscussionService discussionService,
        ILogger<DiscussionsController> logger)
    {
        _discussionService = discussionService;
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
            var discussion = await _discussionService.GetByIdAsync(id);
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
            var discussions = await _discussionService.GetByLessonIdAsync(lessonId, page, pageSize);
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

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token");

        return userId;
    }
}