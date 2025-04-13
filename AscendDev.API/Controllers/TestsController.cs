using AscendDev.Core.DTOs.CodeExecution;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.CodeExecution;
using Microsoft.AspNetCore.Mvc;

namespace AscendDev.Functions.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestsController(ICodeTestService codeTestService) : ControllerBase
{
    [HttpPost("run")]
    public async Task<ActionResult<TestResult>> RunTest([FromBody] RunTestsRequest request)
    {
        if (string.IsNullOrEmpty(request.LessonId) || string.IsNullOrEmpty(request.Code))
            return BadRequest("LessonId and Code are required");

        var result = await codeTestService.RunTestsAsync(request.LessonId, request.Code);

        return Ok(result);
    }
}