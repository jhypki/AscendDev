using AscendDev.Core.DTOs;
using AscendDev.Core.DTOs.Courses;
using AscendDev.Core.Filters;
using AscendDev.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AscendDev.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[ValidateModel]
[Authorize]
public partial class CoursesController(ICourseService courseService, ILessonService lessonService) : ControllerBase
{
    #region Read Operations

    [HttpGet]
    [ProducesResponseType(typeof(List<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllCourses()
    {
        var courses = await courseService.GetAllCourses();
        return Ok(courses);
    }

    [HttpGet("published")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPublishedCourses()
    {
        var courses = await courseService.GetPublishedCourses();
        return Ok(courses);
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(List<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchCourses(
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? language = null,
        [FromQuery] List<string>? tags = null,
        [FromQuery] string? status = null)
    {
        var courses = await courseService.SearchCourses(searchTerm ?? "", language, tags, status);
        return Ok(courses);
    }

    [HttpGet("paginated")]
    [ProducesResponseType(typeof(List<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPaginatedCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool ascending = true)
    {
        var courses = await courseService.GetPaginatedCourses(page, pageSize, sortBy, ascending);
        return Ok(courses);
    }

    [HttpGet("by-language/{language}")]
    [ProducesResponseType(typeof(List<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCoursesByLanguage([FromRoute] string language)
    {
        var courses = await courseService.GetCoursesByLanguage(language);
        return Ok(courses);
    }

    [HttpGet("by-status/{status}")]
    [ProducesResponseType(typeof(List<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCoursesByStatus([FromRoute] string status)
    {
        var courses = await courseService.GetCoursesByStatus(status);
        return Ok(courses);
    }

    [HttpGet("by-creator/{creatorId}")]
    [ProducesResponseType(typeof(List<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCoursesByCreator([FromRoute] string creatorId)
    {
        var courses = await courseService.GetCoursesByCreator(creatorId);
        return Ok(courses);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CourseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCourseById([FromRoute] string id)
    {
        var course = await courseService.GetCourseById(id);
        if (course == null)
            return NotFound($"Course with ID {id} not found");

        // Track view if not the creator
        var userId = GetCurrentUserId();
        if (userId != course.CreatedBy)
        {
            await courseService.IncrementCourseViews(id);
        }

        return Ok(course);
    }

    [HttpGet("slug/{slug}")]
    [ProducesResponseType(typeof(CourseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCourseBySlug([FromRoute] string slug)
    {
        var course = await courseService.GetCourseBySlug(slug);
        if (course == null)
            return NotFound($"Course with slug {slug} not found");

        return Ok(course);
    }

    #endregion

    #region CRUD Operations

    [HttpPost]
    [ProducesResponseType(typeof(CourseResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request)
    {
        var userId = GetCurrentUserId();
        var course = await courseService.CreateCourse(request, userId);
        return CreatedAtAction(nameof(GetCourseById), new { id = course.Id }, course);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CourseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateCourse([FromRoute] string id, [FromBody] UpdateCourseRequest request)
    {
        var userId = GetCurrentUserId();
        var course = await courseService.UpdateCourse(id, request, userId);
        if (course == null)
            return NotFound($"Course with ID {id} not found");

        return Ok(course);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCourse([FromRoute] string id)
    {
        var userId = GetCurrentUserId();
        var result = await courseService.DeleteCourse(id, userId);
        if (!result)
            return NotFound($"Course with ID {id} not found");

        return NoContent();
    }

    #endregion

    #region Versioning Operations

    [HttpPost("{id}/versions")]
    [ProducesResponseType(typeof(CourseResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCourseVersion([FromRoute] string id)
    {
        var userId = GetCurrentUserId();
        var newVersion = await courseService.CreateCourseVersion(id, userId);
        return CreatedAtAction(nameof(GetCourseById), new { id = newVersion.Id }, newVersion);
    }

    [HttpGet("{id}/versions")]
    [ProducesResponseType(typeof(List<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCourseVersions([FromRoute] string id)
    {
        var versions = await courseService.GetCourseVersions(id);
        return Ok(versions);
    }

    [HttpGet("{id}/versions/latest")]
    [ProducesResponseType(typeof(CourseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLatestCourseVersion([FromRoute] string id)
    {
        var latestVersion = await courseService.GetLatestCourseVersion(id);
        if (latestVersion == null)
            return NotFound($"No versions found for course {id}");

        return Ok(latestVersion);
    }

    #endregion

    #region Publishing Workflow

    [HttpPost("{id}/publish")]
    [ProducesResponseType(typeof(CourseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PublishCourse([FromRoute] string id)
    {
        var userId = GetCurrentUserId();
        var course = await courseService.PublishCourse(id, userId);
        if (course == null)
            return NotFound($"Course with ID {id} not found");

        return Ok(course);
    }

    [HttpPost("{id}/unpublish")]
    [ProducesResponseType(typeof(CourseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UnpublishCourse([FromRoute] string id)
    {
        var userId = GetCurrentUserId();
        var course = await courseService.UnpublishCourse(id, userId);
        if (course == null)
            return NotFound($"Course with ID {id} not found");

        return Ok(course);
    }

    [HttpGet("{id}/preview")]
    [ProducesResponseType(typeof(CourseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PreviewCourse([FromRoute] string id)
    {
        var userId = GetCurrentUserId();
        var course = await courseService.PreviewCourse(id, userId);
        if (course == null)
            return NotFound($"Course with ID {id} not found");

        return Ok(course);
    }

    #endregion

    #region Validation Operations

    [HttpPost("{id}/validate")]
    [ProducesResponseType(typeof(CourseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ValidateCourse([FromRoute] string id)
    {
        var userId = GetCurrentUserId();
        var course = await courseService.ValidateCourse(id, userId);
        if (course == null)
            return NotFound($"Course with ID {id} not found");

        return Ok(course);
    }

    [HttpGet("{id}/validation-errors")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCourseValidationErrors([FromRoute] string id)
    {
        var errors = await courseService.GetCourseValidationErrors(id);
        return Ok(errors);
    }

    #endregion

    #region Analytics Operations

    [HttpGet("{id}/analytics")]
    [ProducesResponseType(typeof(CourseAnalyticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCourseAnalytics([FromRoute] string id)
    {
        var analytics = await courseService.GetCourseAnalytics(id);
        if (analytics == null)
            return NotFound($"Analytics not found for course {id}");

        return Ok(analytics);
    }

    [HttpPost("{id}/enroll")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> EnrollInCourse([FromRoute] string id)
    {
        var result = await courseService.TrackCourseEnrollment(id);
        if (!result)
            return NotFound($"Course with ID {id} not found");

        return Ok(new { message = "Successfully enrolled in course" });
    }

    #endregion

    #region Lesson Operations

    [HttpGet("{id}/lessons")]
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

    [HttpGet("{courseId}/lessons/{id}")]
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

    #endregion

    #region Private Helper Methods

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
               User.FindFirst("sub")?.Value ??
               throw new UnauthorizedAccessException("User ID not found in token");
    }

    #endregion
}
