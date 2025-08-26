using AscendDev.Core.DTOs.Courses;
using AscendDev.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AscendDev.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubmissionsController : ControllerBase
{
    private readonly ISubmissionService _submissionService;

    public SubmissionsController(ISubmissionService submissionService)
    {
        _submissionService = submissionService;
    }

    /// <summary>
    /// Get public submissions for a specific lesson
    /// </summary>
    /// <param name="lessonId">The lesson ID</param>
    /// <param name="limit">Maximum number of submissions to return (default: 50)</param>
    /// <returns>List of public submissions for the lesson</returns>
    [HttpGet("lesson/{lessonId}/public")]
    public async Task<ActionResult<IEnumerable<PublicSubmissionResponse>>> GetPublicSubmissionsForLesson(
        string lessonId,
        [FromQuery] int limit = 50)
    {
        if (limit > 100) limit = 100; // Cap the limit to prevent abuse

        var submissions = await _submissionService.GetPublicSubmissionsForLessonAsync(lessonId, limit);
        return Ok(submissions);
    }

    /// <summary>
    /// Get public submissions for a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="limit">Maximum number of submissions to return (default: 50)</param>
    /// <returns>List of public submissions for the user</returns>
    [HttpGet("user/{userId}/public")]
    public async Task<ActionResult<IEnumerable<PublicSubmissionResponse>>> GetUserPublicSubmissions(
        Guid userId,
        [FromQuery] int limit = 50)
    {
        if (limit > 100) limit = 100; // Cap the limit to prevent abuse

        var submissions = await _submissionService.GetUserPublicSubmissionsAsync(userId, limit);
        return Ok(submissions);
    }

    /// <summary>
    /// Get current user's submissions (requires authentication)
    /// </summary>
    /// <returns>List of user's submissions</returns>
    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<SubmissionResponse>>> GetMySubmissions()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user ID in token");
        }

        var submissions = await _submissionService.GetUserSubmissionsAsync(userId);
        return Ok(submissions);
    }

    /// <summary>
    /// Get a specific submission by ID (requires authentication and ownership)
    /// </summary>
    /// <param name="id">The submission ID</param>
    /// <returns>The submission details</returns>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<SubmissionResponse>> GetSubmission(int id)
    {
        var submission = await _submissionService.GetSubmissionByIdAsync(id);
        if (submission == null)
        {
            return NotFound("Submission not found");
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user ID in token");
        }

        // Only allow users to view their own submissions
        if (submission.UserId != userId)
        {
            return Forbid("You can only view your own submissions");
        }

        return Ok(submission);
    }

    /// <summary>
    /// Get current user's submission statistics
    /// </summary>
    /// <returns>User's submission statistics</returns>
    [HttpGet("my/stats")]
    [Authorize]
    public async Task<ActionResult<object>> GetMySubmissionStats()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user ID in token");
        }

        var totalSubmissions = await _submissionService.GetUserSubmissionCountAsync(userId);
        var passedSubmissions = await _submissionService.GetUserPassedSubmissionCountAsync(userId);

        return Ok(new
        {
            TotalSubmissions = totalSubmissions,
            PassedSubmissions = passedSubmissions,
            SuccessRate = totalSubmissions > 0 ? (double)passedSubmissions / totalSubmissions * 100 : 0
        });
    }

    /// <summary>
    /// Delete a submission (requires authentication and ownership)
    /// </summary>
    /// <param name="id">The submission ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteSubmission(int id)
    {
        var submission = await _submissionService.GetSubmissionByIdAsync(id);
        if (submission == null)
        {
            return NotFound("Submission not found");
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user ID in token");
        }

        // Only allow users to delete their own submissions
        if (submission.UserId != userId)
        {
            return Forbid("You can only delete your own submissions");
        }

        await _submissionService.DeleteSubmissionAsync(id);
        return NoContent();
    }
}