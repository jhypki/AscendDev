using AscendDev.Core.DTOs;
using AscendDev.Core.DTOs.Courses;
using AscendDev.Core.Filters;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AscendDev.Functions.Controllers;

[Route("api/[controller]")]
[ApiController]
[ValidateModel]
[Authorize]
public class CoursesController(ICourseService courseService, ILessonService lessonService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<Lesson>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllCourses()
    {
        var courses = await courseService.GetAllCourses();
        return Ok(courses);
    }

    [HttpGet]
    [Route("/{id}")]
    [ProducesResponseType(typeof(Course), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCourseById([FromRoute] string id)
    {
        var course = await courseService.GetCourseById(id);
        return Ok(course);
    }

    [HttpGet]
    [Route("{id}/lessons")]
    [ProducesResponseType(typeof(List<LessonResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLessonsByCourseId([FromRoute] string id)
    {
        var lessons = await lessonService.GetLessonsByCourseId(id);
        return Ok(lessons);
    }

    [HttpGet]
    [Route("{courseId}/lessons/{id}")]
    [ProducesResponseType(typeof(LessonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLessonById([FromRoute] string id, string courseId)
    {
        var lesson = await lessonService.GetLessonById(id);
        return Ok(lesson);
    }
}