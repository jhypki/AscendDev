using AscendDev.API.Controllers;
using AscendDev.Core.DTOs.Courses;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.Courses;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace AscendDev.API.Test.Controllers;

[TestFixture]
public class CoursesControllerTests
{
    private Mock<ICourseService> _courseServiceMock;
    private Mock<ILessonService> _lessonServiceMock;
    private CoursesController _controller;

    [SetUp]
    public void Setup()
    {
        _courseServiceMock = new Mock<ICourseService>();
        _lessonServiceMock = new Mock<ILessonService>();
        _controller = new CoursesController(_courseServiceMock.Object, _lessonServiceMock.Object);
    }

    [Test]
    public async Task GetAllCourses_ReturnsOkResult()
    {
        // Arrange
        var courses = new List<Course>
        {
            new Course { Id = "course-1", Title = "Course 1" },
            new Course { Id = "course-2", Title = "Course 2" }
        };

        _courseServiceMock.Setup(x => x.GetAllCourses())
            .ReturnsAsync(courses);

        // Act
        var result = await _controller.GetAllCourses();

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(courses));
    }

    [Test]
    public async Task GetCourseById_ReturnsOkResult()
    {
        // Arrange
        var courseId = "course-1";
        var course = new Course { Id = courseId, Title = "Course 1" };

        _courseServiceMock.Setup(x => x.GetCourseById(courseId))
            .ReturnsAsync(course);

        // Act
        var result = await _controller.GetCourseById(courseId);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(course));
    }

    [Test]
    public async Task GetLessonsByCourseId_ReturnsOkResult()
    {
        // Arrange
        var courseId = "course-1";
        var lessons = new List<LessonResponse>
        {
            new LessonResponse { Id = "lesson-1", Title = "Lesson 1", CourseId = courseId },
            new LessonResponse { Id = "lesson-2", Title = "Lesson 2", CourseId = courseId }
        };

        _lessonServiceMock.Setup(x => x.GetLessonsByCourseId(courseId))
            .ReturnsAsync(lessons);

        // Act
        var result = await _controller.GetLessonsByCourseId(courseId);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(lessons));
    }

    [Test]
    public async Task GetLessonById_ReturnsOkResult()
    {
        // Arrange
        var courseId = "course-1";
        var lessonId = "lesson-1";
        var lesson = new LessonResponse { Id = lessonId, Title = "Lesson 1", CourseId = courseId };

        _lessonServiceMock.Setup(x => x.GetLessonById(lessonId))
            .ReturnsAsync(lesson);

        // Act
        var result = await _controller.GetLessonById(lessonId, courseId);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(lesson));
    }
}