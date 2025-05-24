using AscendDev.Core.Filters;
using AscendDev.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AscendDev.Functions.Controllers;

[ApiController]
[ValidateModel]
[Authorize]
public class LessonsController(ILessonService lessonService) : ControllerBase
{
    // TODO: move this to courses controller
    // [HttpGet]
    // [Route("api/[controller]/{id}")]
    // [ProducesResponseType(typeof(LessonResponse), StatusCodes.Status200OK)]
    // [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    // [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    // [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    // public async Task<IActionResult> GetLessonById([FromRoute] string id)
    // {
    //     var lesson = await lessonService.GetLessonById(id);
    //     return Ok(lesson);
    // }
}