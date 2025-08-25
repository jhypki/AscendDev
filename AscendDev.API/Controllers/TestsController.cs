using System.Security.Claims;
using AscendDev.Core.DTOs.CodeExecution;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.CodeExecution;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AscendDev.Functions.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TestsController(ICodeTestService codeTestService, ILogger<TestsController> logger) : ControllerBase
{
    [HttpPost("run")]
    public async Task<ActionResult<TestResult>> RunTest([FromBody] RunTestsRequest request)
    {
        if (string.IsNullOrEmpty(request.LessonId))
            return BadRequest("LessonId is required");

        if (!request.IsValid)
            return BadRequest("Either Code or EditableRegions must be provided");

        // Get user ID from claims if user is authenticated
        Guid? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var parsedUserId))
            {
                userId = parsedUserId;
                logger.LogInformation("User {UserId} is running tests for lesson {LessonId} (Template-based: {IsTemplate})",
                    userId, request.LessonId, request.IsTemplateBasedSubmission);
            }
        }

        var result = await codeTestService.RunTestsAsync(request, userId);

        return Ok(result);
    }

    [Authorize]
    [HttpPost("run-authenticated")]
    public async Task<ActionResult<TestResult>> RunTestAuthenticated([FromBody] RunTestsRequest request)
    {
        if (string.IsNullOrEmpty(request.LessonId))
            return BadRequest("LessonId is required");

        if (!request.IsValid)
            return BadRequest("Either Code or EditableRegions must be provided");

        // Get user ID from claims (this endpoint requires authentication)
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            logger.LogWarning("User ID claim missing or invalid in authenticated request");
            return BadRequest("Invalid user authentication");
        }

        logger.LogInformation("User {UserId} is running tests for lesson {LessonId} (Template-based: {IsTemplate})",
            userId, request.LessonId, request.IsTemplateBasedSubmission);
        var result = await codeTestService.RunTestsAsync(request, userId);

        return Ok(result);
    }
}