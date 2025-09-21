using AscendDev.Core.DTOs;
using AscendDev.Core.DTOs.Courses;
using AscendDev.Core.DTOs.Lessons;
using AscendDev.Core.Filters;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AscendDev.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[ValidateModel]
[Authorize]
public class CoursesController(ICourseService courseService, ILessonService lessonService, IUserProgressService userProgressService) : ControllerBase
{
    #region Course CRUD Operations

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedCoursesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllCourses([FromQuery] CourseQueryRequest request)
    {
        var courses = await courseService.GetCoursesAsync(request);
        return Ok(courses);
    }

    [HttpGet("{id}")]
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

    [HttpGet("slug/{slug}")]
    [ProducesResponseType(typeof(Course), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCourseBySlug([FromRoute] string slug)
    {
        var course = await courseService.GetCourseBySlug(slug);
        return Ok(course);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Course), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request)
    {
        var course = new Course
        {
            Title = request.Title,
            Slug = request.Slug,
            Description = request.Description,
            Language = request.Language,
            Tags = request.Tags,
            FeaturedImage = request.FeaturedImage,
            Status = request.Status
        };

        var createdCourse = await courseService.CreateCourse(course);
        return CreatedAtAction(nameof(GetCourseById), new { id = createdCourse.Id }, createdCourse);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Course), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateCourse([FromRoute] string id, [FromBody] UpdateCourseRequest request)
    {
        var existingCourse = await courseService.GetCourseById(id);
        if (existingCourse == null)
            return NotFound();

        // Update only provided fields
        if (request.Title != null) existingCourse.Title = request.Title;
        if (request.Slug != null) existingCourse.Slug = request.Slug;
        if (request.Description != null) existingCourse.Description = request.Description;
        if (request.Language != null) existingCourse.Language = request.Language;
        if (request.Tags != null) existingCourse.Tags = request.Tags;
        if (request.FeaturedImage != null) existingCourse.FeaturedImage = request.FeaturedImage;
        if (request.Status != null) existingCourse.Status = request.Status;

        var updatedCourse = await courseService.UpdateCourse(existingCourse);
        return Ok(updatedCourse);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCourse([FromRoute] string id)
    {
        var result = await courseService.DeleteCourse(id);
        return result ? NoContent() : NotFound();
    }

    #endregion

    #region Course Publishing Workflow

    [HttpPost("{id}/publish")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PublishCourse([FromRoute] string id)
    {
        var result = await courseService.PublishCourse(id);
        return result ? Ok(new { message = "Course published successfully" }) : BadRequest(new { message = "Failed to publish course" });
    }

    [HttpPost("{id}/unpublish")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UnpublishCourse([FromRoute] string id)
    {
        var result = await courseService.UnpublishCourse(id);
        return result ? Ok(new { message = "Course unpublished successfully" }) : BadRequest(new { message = "Failed to unpublish course" });
    }

    [HttpGet("{id}/preview")]
    [ProducesResponseType(typeof(Course), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PreviewCourse([FromRoute] string id)
    {
        var course = await courseService.PreviewCourse(id);
        return Ok(course);
    }

    #endregion

    #region Course Filtering and Search

    [HttpGet("published")]
    [ProducesResponseType(typeof(List<Course>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPublishedCourses()
    {
        var courses = await courseService.GetPublishedCourses();
        return Ok(courses);
    }

    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(List<Course>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCoursesByStatus([FromRoute] string status)
    {
        var courses = await courseService.GetCoursesByStatus(status);
        return Ok(courses);
    }

    [HttpGet("tag/{tag}")]
    [ProducesResponseType(typeof(List<Course>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCoursesByTag([FromRoute] string tag)
    {
        var courses = await courseService.GetCoursesByTag(tag);
        return Ok(courses);
    }

    [HttpGet("language/{language}")]
    [ProducesResponseType(typeof(List<Course>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCoursesByLanguage([FromRoute] string language)
    {
        var courses = await courseService.GetCoursesByLanguage(language);
        return Ok(courses);
    }

    #endregion

    #region Course Analytics

    [HttpGet("analytics/count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTotalCourseCount()
    {
        var count = await courseService.GetTotalCourseCount();
        return Ok(count);
    }

    [HttpGet("analytics/count/status/{status}")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCourseCountByStatus([FromRoute] string status)
    {
        var count = await courseService.GetCourseCountByStatus(status);
        return Ok(count);
    }

    [HttpGet("analytics/statistics")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCourseStatistics()
    {
        var statistics = await courseService.GetCourseStatistics();
        return Ok(statistics);
    }

    #endregion

    #region Course Validation

    [HttpPost("{id}/validate")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ValidateCourse([FromRoute] string id)
    {
        var course = await courseService.GetCourseById(id);
        if (course == null)
            return NotFound();

        var isValid = await courseService.ValidateCourse(course);
        return Ok(isValid);
    }

    [HttpPost("{id}/validation-errors")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCourseValidationErrors([FromRoute] string id)
    {
        var course = await courseService.GetCourseById(id);
        if (course == null)
            return NotFound();

        var errors = await courseService.GetCourseValidationErrors(course);
        return Ok(errors);
    }

    #endregion

    #region Lesson Management

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

    [HttpGet("{id}/lessons/ordered")]
    [ProducesResponseType(typeof(List<LessonResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLessonsByCourseIdOrdered([FromRoute] string id)
    {
        var lessons = await lessonService.GetLessonsByCourseIdOrdered(id);
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

    [HttpPost("{id}/lessons")]
    [ProducesResponseType(typeof(LessonResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateLesson([FromRoute] string id, [FromBody] CreateLessonRequest request)
    {
        var lesson = new Lesson
        {
            CourseId = id,
            Title = request.Title,
            Slug = request.Slug,
            Content = request.Content,
            Language = request.Language,
            Template = request.Template,
            Order = request.Order,
            Tags = request.Tags,
            TestConfig = request.TestConfig ?? new TestConfig(),
            AdditionalResources = request.AdditionalResources,
            Status = request.Status
        };

        var createdLesson = await lessonService.CreateLesson(lesson);
        return CreatedAtAction(nameof(GetLessonById), new { id = createdLesson.Id, courseId = id }, createdLesson);
    }

    [HttpPut("{courseId}/lessons/{id}")]
    [ProducesResponseType(typeof(LessonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateLesson([FromRoute] string id, [FromRoute] string courseId, [FromBody] UpdateLessonRequest request)
    {
        // Get the existing lesson as a domain model, not DTO
        var existingLessonResponse = await lessonService.GetLessonById(id);
        if (existingLessonResponse == null)
            return NotFound();

        // Convert LessonResponse back to Lesson for updating
        var existingLesson = new Lesson
        {
            Id = existingLessonResponse.Id,
            CourseId = existingLessonResponse.CourseId,
            Title = existingLessonResponse.Title,
            Slug = existingLessonResponse.Slug,
            Content = existingLessonResponse.Content,
            Language = existingLessonResponse.Language,
            Template = existingLessonResponse.Template,
            CreatedAt = existingLessonResponse.CreatedAt,
            UpdatedAt = existingLessonResponse.UpdatedAt,
            Order = existingLessonResponse.Order,
            AdditionalResources = existingLessonResponse.AdditionalResources,
            Tags = existingLessonResponse.Tags,
            Status = "draft" // Default status since LessonResponse doesn't have Status
        };

        // Update only provided fields
        if (request.Title != null) existingLesson.Title = request.Title;
        if (request.Slug != null) existingLesson.Slug = request.Slug;
        if (request.Content != null) existingLesson.Content = request.Content;
        if (request.Language != null) existingLesson.Language = request.Language;
        if (request.Template != null) existingLesson.Template = request.Template;
        if (request.Order.HasValue) existingLesson.Order = request.Order.Value;
        if (request.Tags != null) existingLesson.Tags = request.Tags;
        if (request.TestConfig != null) existingLesson.TestConfig = request.TestConfig;
        if (request.AdditionalResources != null) existingLesson.AdditionalResources = request.AdditionalResources;
        if (request.Status != null) existingLesson.Status = request.Status;

        var updatedLesson = await lessonService.UpdateLesson(existingLesson);
        return Ok(updatedLesson);
    }

    [HttpDelete("{courseId}/lessons/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteLesson([FromRoute] string id, [FromRoute] string courseId)
    {
        var result = await lessonService.DeleteLesson(id);
        return result ? NoContent() : NotFound();
    }

    [HttpPost("{courseId}/lessons/{id}/reorder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ReorderLessons([FromRoute] string courseId, [FromBody] List<string> lessonIds)
    {
        var result = await lessonService.ReorderLessons(courseId, lessonIds);
        return result ? Ok(new { message = "Lessons reordered successfully" }) : BadRequest(new { message = "Failed to reorder lessons" });
    }

    [HttpGet("{courseId}/lessons/{id}/preview")]
    [ProducesResponseType(typeof(LessonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PreviewLesson([FromRoute] string id, [FromRoute] string courseId)
    {
        var lesson = await lessonService.PreviewLesson(id);
        return Ok(lesson);
    }

    #endregion

    #region User Progress

    /// <summary>
    /// Get user progress for a specific course
    /// </summary>
    [HttpGet("{courseId}/progress")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCourseProgress(string courseId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user ID");
        }

        // Get course to verify it exists and get lesson count
        var course = await courseService.GetCourseById(courseId);
        if (course == null)
        {
            return NotFound("Course not found");
        }

        // Get lessons for the course
        var lessons = await lessonService.GetLessonsByCourseId(courseId);
        var totalLessons = lessons.Count;

        // Get user progress for this course
        var completedCount = await userProgressService.GetCompletedLessonCountForCourseAsync(userId, courseId);
        var completionPercentage = await userProgressService.GetCourseCompletionPercentageAsync(userId, courseId);

        // Get all user progress to determine which specific lessons are completed
        var userProgress = await userProgressService.GetUserProgressAsync(userId);
        var completedLessonIds = userProgress
            .Where(p => lessons.Any(l => l.Id == p.LessonId))
            .Select(p => p.LessonId)
            .ToList();

        var progressResponse = new
        {
            CourseId = courseId,
            UserId = userId,
            TotalLessons = totalLessons,
            CompletedLessons = completedCount,
            CompletionPercentage = completionPercentage,
            CompletedLessonIds = completedLessonIds
        };

        return Ok(progressResponse);
    }

    /// <summary>
    /// Mark a lesson as completed
    /// </summary>
    [HttpPost("{courseId}/lessons/{lessonId}/complete")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkLessonCompleted(string courseId, string lessonId, [FromBody] MarkLessonCompletedRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user ID");
        }

        // Verify lesson exists and belongs to the course
        var lesson = await lessonService.GetLessonById(lessonId);
        if (lesson == null || lesson.CourseId != courseId)
        {
            return NotFound("Lesson not found in this course");
        }

        // Check if already completed
        var isCompleted = await userProgressService.HasUserCompletedLessonAsync(userId, lessonId);
        if (isCompleted)
        {
            return BadRequest("Lesson already completed");
        }

        // Mark as completed (use 0 as submission ID for manual completion)
        var progress = await userProgressService.MarkLessonAsCompletedAsync(userId, lessonId, 0);

        return Ok(new
        {
            LessonId = lessonId,
            CompletedAt = progress.CompletedAt,
            Message = "Lesson marked as completed"
        });
    }

    #endregion
}

public class MarkLessonCompletedRequest
{
    public string? CodeSolution { get; set; }
}
