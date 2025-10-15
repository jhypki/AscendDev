using AscendDev.Core.DTOs.Social;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.Social;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AscendDev.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class CodeReviewsController : ControllerBase
{
    private readonly ICodeReviewService _codeReviewService;
    private readonly ICodeReviewCommentService _codeReviewCommentService;

    public CodeReviewsController(
        ICodeReviewService codeReviewService,
        ICodeReviewCommentService codeReviewCommentService)
    {
        _codeReviewService = codeReviewService;
        _codeReviewCommentService = codeReviewCommentService;
    }

    /// <summary>
    /// Get code reviews for a specific lesson
    /// </summary>
    /// <param name="lessonId">The lesson ID</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>List of code reviews for the lesson</returns>
    [HttpGet("lesson/{lessonId}")]
    public async Task<ActionResult<IEnumerable<CodeReviewResponse>>> GetCodeReviewsByLesson(
        string lessonId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var codeReviews = await _codeReviewService.GetByLessonIdAsync(lessonId, page, pageSize);
        return Ok(codeReviews);
    }

    /// <summary>
    /// Get code reviews where current user is the reviewer
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>List of code reviews where user is reviewer</returns>
    [HttpGet("my-reviews")]
    public async Task<ActionResult<IEnumerable<CodeReviewResponse>>> GetMyReviews(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user ID in token");
        }

        var codeReviews = await _codeReviewService.GetByReviewerIdAsync(userId, page, pageSize);
        return Ok(codeReviews);
    }

    /// <summary>
    /// Get code reviews where current user is the reviewee (submissions being reviewed)
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>List of code reviews where user is reviewee</returns>
    [HttpGet("my-submissions")]
    public async Task<ActionResult<IEnumerable<CodeReviewResponse>>> GetMySubmissions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user ID in token");
        }

        var codeReviews = await _codeReviewService.GetByRevieweeIdAsync(userId, page, pageSize);
        return Ok(codeReviews);
    }

    /// <summary>
    /// Get pending code reviews (available for review)
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>List of pending code reviews</returns>
    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<CodeReviewResponse>>> GetPendingReviews(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var codeReviews = await _codeReviewService.GetPendingReviewsAsync(page, pageSize);
        return Ok(codeReviews);
    }

    /// <summary>
    /// Get a specific code review by ID
    /// </summary>
    /// <param name="id">The code review ID</param>
    /// <returns>The code review details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<CodeReviewResponse>> GetCodeReview(Guid id)
    {
        var codeReview = await _codeReviewService.GetByIdAsync(id);
        if (codeReview == null)
        {
            return NotFound("Code review not found");
        }

        return Ok(codeReview);
    }

    /// <summary>
    /// Get existing code review for a submission by current user
    /// </summary>
    /// <param name="submissionId">The submission ID</param>
    /// <returns>The code review if exists, null otherwise</returns>
    [HttpGet("submission/{submissionId}/my-review")]
    public async Task<ActionResult<CodeReviewResponse>> GetMyCodeReviewForSubmission(int submissionId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user ID in token");
        }

        var codeReview = await _codeReviewService.GetBySubmissionAndReviewerAsync(submissionId, userId);
        if (codeReview == null)
        {
            return NotFound("Code review not found");
        }

        return Ok(codeReview);
    }

    /// <summary>
    /// Get all code reviews for a submission (for viewing all comments)
    /// </summary>
    /// <param name="submissionId">The submission ID</param>
    /// <returns>All code reviews for the submission</returns>
    [HttpGet("submission/{submissionId}/all")]
    public async Task<ActionResult<IEnumerable<CodeReviewResponse>>> GetAllCodeReviewsForSubmission(int submissionId)
    {
        try
        {
            var codeReviews = await _codeReviewService.GetBySubmissionIdAsync(submissionId);
            return Ok(codeReviews);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error retrieving code reviews for submission: {ex.Message}");
        }
    }

    /// <summary>
    /// Get all comments for a submission (from all code reviews)
    /// </summary>
    /// <param name="submissionId">The submission ID</param>
    /// <returns>All comments for the submission</returns>
    [HttpGet("submission/{submissionId}/comments")]
    public async Task<ActionResult<IEnumerable<CodeReviewCommentResponse>>> GetAllCommentsForSubmission(int submissionId)
    {
        try
        {
            var codeReviews = await _codeReviewService.GetBySubmissionIdAsync(submissionId);
            var allComments = new List<CodeReviewCommentResponse>();

            foreach (var review in codeReviews)
            {
                var comments = await _codeReviewCommentService.GetByCodeReviewIdAsync(review.Id);
                allComments.AddRange(comments);
            }

            // Sort by creation date
            return Ok(allComments.OrderBy(c => c.CreatedAt));
        }
        catch (Exception ex)
        {
            return BadRequest($"Error retrieving comments for submission: {ex.Message}");
        }
    }

    /// <summary>
    /// Update a code review (change status, etc.)
    /// </summary>
    /// <param name="id">The code review ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated code review</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<CodeReviewResponse>> UpdateCodeReview(Guid id, [FromBody] UpdateCodeReviewRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user ID in token");
        }

        var updatedCodeReview = await _codeReviewService.UpdateAsync(id, request, userId);
        return Ok(updatedCodeReview);
    }

    /// <summary>
    /// Delete a code review
    /// </summary>
    /// <param name="id">The code review ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCodeReview(Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user ID in token");
        }

        var success = await _codeReviewService.DeleteAsync(id, userId);
        if (!success)
        {
            return NotFound("Code review not found");
        }

        return NoContent();
    }

    /// <summary>
    /// Create a new code review for a submission
    /// </summary>
    /// <param name="request">Create code review request</param>
    /// <returns>Created code review</returns>
    [HttpPost]
    public async Task<ActionResult<CodeReviewResponse>> CreateCodeReview([FromBody] CreateCodeReviewRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user ID in token");
        }

        var codeReview = await _codeReviewService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetCodeReview), new { id = codeReview.Id }, codeReview);
    }

    /// <summary>
    /// Get comments for a specific code review
    /// </summary>
    /// <param name="id">The code review ID</param>
    /// <returns>List of comments for the code review</returns>
    [HttpGet("{id}/comments")]
    public async Task<ActionResult<IEnumerable<CodeReviewCommentResponse>>> GetCodeReviewComments(Guid id)
    {
        var comments = await _codeReviewCommentService.GetByCodeReviewIdAsync(id);
        return Ok(comments);
    }

    /// <summary>
    /// Add a comment to a code review
    /// </summary>
    /// <param name="id">The code review ID</param>
    /// <param name="request">Create comment request</param>
    /// <returns>Created comment</returns>
    [HttpPost("{id}/comments")]
    public async Task<ActionResult<CodeReviewCommentResponse>> CreateCodeReviewComment(
        Guid id,
        [FromBody] CreateCodeReviewCommentRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user ID in token");
        }

        var comment = await _codeReviewCommentService.CreateAsync(id, request, userId);
        return CreatedAtAction(nameof(GetCodeReviewComment), new { id, commentId = comment.Id }, comment);
    }

    /// <summary>
    /// Get a specific comment
    /// </summary>
    /// <param name="id">The code review ID</param>
    /// <param name="commentId">The comment ID</param>
    /// <returns>The comment details</returns>
    [HttpGet("{id}/comments/{commentId}")]
    public async Task<ActionResult<CodeReviewCommentResponse>> GetCodeReviewComment(Guid id, Guid commentId)
    {
        var comment = await _codeReviewCommentService.GetByIdAsync(commentId);
        if (comment == null)
        {
            return NotFound("Comment not found");
        }
        return Ok(comment);
    }

    /// <summary>
    /// Update a comment
    /// </summary>
    /// <param name="id">The code review ID</param>
    /// <param name="commentId">The comment ID</param>
    /// <param name="request">Update comment request</param>
    /// <returns>Updated comment</returns>
    [HttpPut("{id}/comments/{commentId}")]
    public async Task<ActionResult<CodeReviewCommentResponse>> UpdateCodeReviewComment(
        Guid id,
        Guid commentId,
        [FromBody] UpdateCodeReviewCommentRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user ID in token");
        }

        var comment = await _codeReviewCommentService.UpdateAsync(commentId, request, userId);
        return Ok(comment);
    }

    /// <summary>
    /// Delete a comment
    /// </summary>
    /// <param name="id">The code review ID</param>
    /// <param name="commentId">The comment ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}/comments/{commentId}")]
    public async Task<ActionResult> DeleteCodeReviewComment(Guid id, Guid commentId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user ID in token");
        }

        var success = await _codeReviewCommentService.DeleteAsync(commentId, userId);
        if (!success)
        {
            return NotFound("Comment not found");
        }
        return NoContent();
    }

    /// <summary>
    /// Resolve a comment
    /// </summary>
    /// <param name="id">The code review ID</param>
    /// <param name="commentId">The comment ID</param>
    /// <returns>Success status</returns>
    [HttpPatch("{id}/comments/{commentId}/resolve")]
    public async Task<ActionResult> ResolveCodeReviewComment(Guid id, Guid commentId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user ID in token");
        }

        var request = new UpdateCodeReviewCommentRequest { IsResolved = true };
        await _codeReviewCommentService.UpdateAsync(commentId, request, userId);
        return NoContent();
    }
}