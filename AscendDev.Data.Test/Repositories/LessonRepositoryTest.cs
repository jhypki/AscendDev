using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Courses;
using AscendDev.Data.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace AscendDev.Tests.Data.Repositories;

[TestFixture]
public class LessonRepositoryTest
{
    [SetUp]
    public void Setup()
    {
        _mockSql = new Mock<ISqlExecutor>();
        _mockLogger = new Mock<ILogger<ILessonRepository>>();
        _lessonRepository = new LessonRepository(_mockLogger.Object, _mockSql.Object);
    }

    private Mock<ISqlExecutor> _mockSql;
    private Mock<ILogger<ILessonRepository>> _mockLogger;
    private LessonRepository _lessonRepository;

    [Test]
    public async Task GetByCourseId_WhenLessonsExist_ShouldReturnLessons()
    {
        // Arrange
        var courseId = "course-123";
        var expectedLessons = new List<Lesson>
        {
            CreateTestLesson("lesson-1", courseId),
            CreateTestLesson("lesson-2", courseId)
        };

        _mockSql.Setup(x => x.QueryAsync<Lesson>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(expectedLessons);

        // Act
        var result = await _lessonRepository.GetByCourseId(courseId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].Id, Is.EqualTo("lesson-1"));
        Assert.That(result[1].Id, Is.EqualTo("lesson-2"));
        Assert.That(result[0].CourseId, Is.EqualTo(courseId));
        Assert.That(result[1].CourseId, Is.EqualTo(courseId));

        _mockSql.Verify(x => x.QueryAsync<Lesson>(
                It.IsAny<string>(),
                It.Is<object>(p => p.GetType().GetProperty("CourseId")!.GetValue(p)!.ToString() == courseId)),
            Times.Once);
    }

    [Test]
    public async Task GetByCourseId_WhenNoLessonsExist_ShouldReturnEmptyList()
    {
        // Arrange
        var courseId = "course-123";

        _mockSql.Setup(x => x.QueryAsync<Lesson>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(new List<Lesson>());

        // Act
        var result = await _lessonRepository.GetByCourseId(courseId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0));

        _mockSql.Verify(x => x.QueryAsync<Lesson>(
                It.IsAny<string>(),
                It.Is<object>(p => p.GetType().GetProperty("CourseId")!.GetValue(p)!.ToString() == courseId)),
            Times.Once);
    }

    [Test]
    public Task GetByCourseId_WhenExceptionOccurs_ShouldLogAndRethrow()
    {
        // Arrange
        var courseId = "course-123";
        var exception = new Exception("Database error");

        _mockSql.Setup(x => x.QueryAsync<Lesson>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _lessonRepository.GetByCourseId(courseId));
        Assert.That(ex.Message, Does.Contain("Error retrieving lessons for course"));

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        return Task.CompletedTask;
    }

    [Test]
    public async Task GetById_WhenLessonExists_ShouldReturnLesson()
    {
        // Arrange
        var lessonId = "lesson-123";
        var expectedLesson = CreateTestLesson(lessonId, "course-123");

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<Lesson>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(expectedLesson);

        // Act
        var result = await _lessonRepository.GetById(lessonId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(lessonId));
        Assert.That(result.CourseId, Is.EqualTo("course-123"));

        _mockSql.Verify(x => x.QueryFirstOrDefaultAsync<Lesson>(
                It.IsAny<string>(),
                It.Is<object>(p => p.GetType().GetProperty("Id")!.GetValue(p)!.ToString() == lessonId)),
            Times.Once);
    }

    [Test]
    public async Task GetById_WhenLessonDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var lessonId = "lesson-123";

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<Lesson>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync((Lesson)null!);

        // Act
        var result = await _lessonRepository.GetById(lessonId);

        // Assert
        Assert.That(result, Is.Null);

        _mockSql.Verify(x => x.QueryFirstOrDefaultAsync<Lesson>(
                It.IsAny<string>(),
                It.Is<object>(p => p.GetType().GetProperty("Id")!.GetValue(p)!.ToString() == lessonId)),
            Times.Once);
    }

    [Test]
    public Task GetById_WhenExceptionOccurs_ShouldLogAndRethrow()
    {
        // Arrange
        var lessonId = "lesson-123";
        var exception = new Exception("Database error");

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<Lesson>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _lessonRepository.GetById(lessonId));
        Assert.That(ex.Message, Does.Contain("Error retrieving lesson by ID"));

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        return Task.CompletedTask;
    }

    [Test]
    public async Task GetBySlug_WhenLessonExists_ShouldReturnLesson()
    {
        // Arrange
        var slug = "lesson-slug";
        var expectedLesson = CreateTestLesson("lesson-123", "course-123", slug);

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<Lesson>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(expectedLesson);

        // Act
        var result = await _lessonRepository.GetBySlug(slug);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Slug, Is.EqualTo(slug));
        Assert.That(result.Id, Is.EqualTo("lesson-123"));

        _mockSql.Verify(x => x.QueryFirstOrDefaultAsync<Lesson>(
                It.IsAny<string>(),
                It.Is<object>(p => p.GetType().GetProperty("Slug")!.GetValue(p)!.ToString() == slug)),
            Times.Once);
    }

    [Test]
    public async Task GetBySlug_WhenLessonDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var slug = "lesson-slug";

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<Lesson>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync((Lesson)null!);

        // Act
        var result = await _lessonRepository.GetBySlug(slug);

        // Assert
        Assert.That(result, Is.Null);

        _mockSql.Verify(x => x.QueryFirstOrDefaultAsync<Lesson>(
                It.IsAny<string>(),
                It.Is<object>(p => p.GetType().GetProperty("Slug")!.GetValue(p)!.ToString() == slug)),
            Times.Once);
    }

    [Test]
    public Task GetBySlug_WhenExceptionOccurs_ShouldLogAndRethrow()
    {
        // Arrange
        var slug = "lesson-slug";
        var exception = new Exception("Database error");

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<Lesson>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _lessonRepository.GetBySlug(slug));
        Assert.That(ex.Message, Does.Contain("Error retrieving lesson by slug"));

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        return Task.CompletedTask;
    }

    private static Lesson CreateTestLesson(string id, string courseId, string slug = null)
    {
        return new Lesson
        {
            Id = id,
            CourseId = courseId,
            Title = $"Test Lesson {id}",
            Slug = slug ?? $"test-lesson-{id}",
            Content = "Test content",
            Template = "Test template",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Language = "csharp",
            Order = 1,
            TestConfig = new TestConfig(),
            AdditionalResources = new List<AdditionalResource>(),
            Tags = new List<string> { "test" },
            Status = "published"
        };
    }
}