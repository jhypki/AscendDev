using System.Security.Claims;
using AscendDev.Core.DTOs.CodeExecution;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.CodeExecution;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AscendDev.Functions.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class TestsController(ICodeTestService codeTestService, ILogger<TestsController> logger) : ControllerBase
{
    [HttpPost("run")]
    public async Task<ActionResult<TestResult>> RunTest([FromBody] RunTestsRequest request)
    {
        if (string.IsNullOrEmpty(request.LessonId) || string.IsNullOrEmpty(request.Code))
            return BadRequest("LessonId and Code are required");

        // Get user ID from claims if user is authenticated
        Guid? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var parsedUserId))
            {
                userId = parsedUserId;
                logger.LogInformation("User {UserId} is running tests for lesson {LessonId}", userId, request.LessonId);
            }
        }

        var result = await codeTestService.RunTestsAsync(request.LessonId, request.Code, userId);

        return Ok(result);
    }
}