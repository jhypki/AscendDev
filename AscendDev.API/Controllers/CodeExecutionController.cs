using AscendDev.Core.DTOs.CodeExecution;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.CodeExecution;
using Microsoft.AspNetCore.Mvc;

namespace AscendDev.Functions.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CodeExecutionController(ICodeExecutionService codeExecutionService) : ControllerBase
{
    [HttpPost("run")]
    public async Task<ActionResult<CodeExecutionResult>> RunCode([FromBody] RunCodeRequest request)
    {
        if (string.IsNullOrEmpty(request.Language) || string.IsNullOrEmpty(request.Code))
            return BadRequest("Language and Code are required");

        var result = await codeExecutionService.ExecuteCodeAsync(request.Language, request.Code);

        return Ok(result);
    }
}