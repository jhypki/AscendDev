using AscendDev.Core.DTOs;
using AscendDev.Core.DTOs.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AscendDev.API.Controllers;

public partial class CoursesController
{
    #region Lesson CRUD Operations

    [HttpPost("{courseId}/lessons")]
    [ProducesResponseType(typeof(LessonResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateLesson([FromRoute] string courseId, [FromBody] CreateLessonRequest request)
    {
        var userId = GetCurrentUserId();

        // Ensure the lesson is associated with the correct course
        request.CourseId = courseId;

        var lesson = await lessonService.CreateLesson(request, userId);
        return CreatedAtAction(nameof(GetLessonById), new { courseId = courseId, id = lesson.Id }, lesson);
    }

    [HttpPut("{courseId}/lessons/{id}")]
    [ProducesResponseType(typeof(LessonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateLesson([FromRoute] string courseId, [FromRoute] string id, [FromBody] UpdateLessonRequest request)
    {
        var userId = GetCurrentUserId();
        var lesson = await lessonService.UpdateLesson(id, request, userId);
        if (lesson == null)
            return NotFound($"Lesson with ID {id} not found in course {courseId}");

        return Ok(lesson);
    }

    [HttpDelete("{courseId}/lessons/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteLesson([FromRoute] string courseId, [FromRoute] string id)
    {
        var userId = GetCurrentUserId();
        var result = await lessonService.DeleteLesson(id, userId);
        if (!result)
            return NotFound($"Lesson with ID {id} not found in course {courseId}");

        return NoContent();
    }

    #endregion

    #region Lesson Search and Filtering

    [HttpGet("{courseId}/lessons/search")]
    [ProducesResponseType(typeof(List<LessonResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchLessons(
        [FromRoute] string courseId,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? difficulty = null,
        [FromQuery] List<string>? tags = null)
    {
        var lessons = await lessonService.SearchLessons(searchTerm ?? "", courseId, difficulty, tags);
        return Ok(lessons);
    }

    [HttpGet("{courseId}/lessons/paginated")]
    [ProducesResponseType(typeof(List<LessonResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPaginatedLessons(
        [FromRoute] string courseId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool ascending = true)
    {
        var lessons = await lessonService.GetPaginatedLessons(page, pageSize, courseId, sortBy, ascending);
        return Ok(lessons);
    }

    [HttpGet("{courseId}/lessons/by-difficulty/{difficulty}")]
    [ProducesResponseType(typeof(List<LessonResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLessonsByDifficulty([FromRoute] string courseId, [FromRoute] string difficulty)
    {
        var lessons = await lessonService.GetLessonsByDifficulty(difficulty);
        return Ok(lessons);
    }

    [HttpGet("{courseId}/lessons/by-status/{status}")]
    [ProducesResponseType(typeof(List<LessonResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLessonsByStatus([FromRoute] string courseId, [FromRoute] string status)
    {
        var lessons = await lessonService.GetLessonsByStatus(status);
        return Ok(lessons);
    }

    #endregion

    #region Lesson Ordering and Organization

    [HttpPost("{courseId}/lessons/reorder")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ReorderLessons([FromRoute] string courseId, [FromBody] List<string> lessonIds)
    {
        var userId = GetCurrentUserId();
        var result = await lessonService.ReorderLessons(courseId, lessonIds, userId);
        return Ok(result);
    }

    [HttpGet("{courseId}/lessons/ordered")]
    [ProducesResponseType(typeof(List<LessonResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderedLessons([FromRoute] string courseId)
    {
        var lessons = await lessonService.GetOrderedLessonsByCourseId(courseId);
        return Ok(lessons);
    }

    #endregion

    #region Lesson Publishing Workflow

    [HttpPost("{courseId}/lessons/{id}/publish")]
    [ProducesResponseType(typeof(LessonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PublishLesson([FromRoute] string courseId, [FromRoute] string id)
    {
        var userId = GetCurrentUserId();
        var lesson = await lessonService.PublishLesson(id, userId);
        if (lesson == null)
            return NotFound($"Lesson with ID {id} not found in course {courseId}");

        return Ok(lesson);
    }

    [HttpPost("{courseId}/lessons/{id}/unpublish")]
    [ProducesResponseType(typeof(LessonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UnpublishLesson([FromRoute] string courseId, [FromRoute] string id)
    {
        var userId = GetCurrentUserId();
        var lesson = await lessonService.UnpublishLesson(id, userId);
        if (lesson == null)
            return NotFound($"Lesson with ID {id} not found in course {courseId}");

        return Ok(lesson);
    }

    [HttpGet("{courseId}/lessons/published")]
    [ProducesResponseType(typeof(List<LessonResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPublishedLessons([FromRoute] string courseId)
    {
        var lessons = await lessonService.GetPublishedLessons();
        return Ok(lessons);
    }

    [HttpGet("{courseId}/lessons/{id}/preview")]
    [ProducesResponseType(typeof(LessonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PreviewLesson([FromRoute] string courseId, [FromRoute] string id)
    {
        var userId = GetCurrentUserId();
        var lesson = await lessonService.PreviewLesson(id, userId);
        if (lesson == null)
            return NotFound($"Lesson with ID {id} not found in course {courseId}");

        return Ok(lesson);
    }

    #endregion

    #region Lesson Validation Operations

    [HttpPost("{courseId}/lessons/{id}/validate")]
    [ProducesResponseType(typeof(LessonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ValidateLesson([FromRoute] string courseId, [FromRoute] string id)
    {
        var userId = GetCurrentUserId();
        var lesson = await lessonService.ValidateLesson(id, userId);
        if (lesson == null)
            return NotFound($"Lesson with ID {id} not found in course {courseId}");

        return Ok(lesson);
    }

    [HttpGet("{courseId}/lessons/{id}/validation-errors")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLessonValidationErrors([FromRoute] string courseId, [FromRoute] string id)
    {
        var errors = await lessonService.GetLessonValidationErrors(id);
        return Ok(errors);
    }

    #endregion

    #region Lesson Analytics Operations

    [HttpPost("{courseId}/lessons/{id}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CompleteLesson([FromRoute] string courseId, [FromRoute] string id, [FromBody] double timeSpentMinutes)
    {
        var userId = GetCurrentUserId();
        var result = await lessonService.TrackLessonCompletion(id, userId, timeSpentMinutes);
        if (!result)
            return NotFound($"Lesson with ID {id} not found in course {courseId}");

        return Ok(new { message = "Lesson completion tracked successfully" });
    }

    [HttpPost("{courseId}/lessons/{id}/view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> TrackLessonView([FromRoute] string courseId, [FromRoute] string id)
    {
        var result = await lessonService.IncrementLessonViews(id);
        if (!result)
            return NotFound($"Lesson with ID {id} not found in course {courseId}");

        return Ok(new { message = "Lesson view tracked successfully" });
    }

    #endregion

    #region Lesson Prerequisites

    [HttpGet("{courseId}/lessons/{id}/prerequisites")]
    [ProducesResponseType(typeof(List<LessonResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLessonPrerequisites([FromRoute] string courseId, [FromRoute] string id)
    {
        var prerequisites = await lessonService.GetLessonPrerequisites(id);
        return Ok(prerequisites);
    }

    [HttpGet("{courseId}/lessons/{id}/can-access")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CanAccessLesson([FromRoute] string courseId, [FromRoute] string id)
    {
        var userId = GetCurrentUserId();
        var canAccess = await lessonService.CanAccessLesson(id, userId);
        return Ok(canAccess);
    }

    #endregion
}