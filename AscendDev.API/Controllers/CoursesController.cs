using AscendDev.Core.DTOs;
using AscendDev.Core.DTOs.Courses;
using AscendDev.Core.Filters;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AscendDev.Functions.Controllers;

[ApiController]
[ValidateModel]
[Authorize]
public class CoursesController(ICourseService courseService, ILessonService lessonService) : ControllerBase
{
    [HttpGet]
    [Route("api/[controller]")]
    [ProducesResponseType(typeof(SuccessApiResponse<List<Lesson>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllCourses()
    {
        var courses = await courseService.GetAllCourses();
        return Ok(ApiResponse<List<Course>>.SuccessResponse(courses));
    }

    [HttpGet]
    [Route("api/[controller]/{id}")]
    [ProducesResponseType(typeof(SuccessApiResponse<Course>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCourseById([FromRoute] string id)
    {
        var course = await courseService.GetCourseById(id);
        return Ok(ApiResponse<Course>.SuccessResponse(course));
    }

    [HttpGet]
    [Route("api/[controller]/{id}/lessons")]
    [ProducesResponseType(typeof(SuccessApiResponse<List<LessonResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLessonsByCourseId([FromRoute] string id)
    {
        var lessons = await lessonService.GetLessonsByCourseId(id);
        return Ok(ApiResponse<List<LessonResponse>>.SuccessResponse(lessons));
    }
}