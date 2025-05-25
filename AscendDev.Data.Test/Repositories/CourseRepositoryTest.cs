using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Courses;
using AscendDev.Data.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace AscendDev.Tests.Data.Repositories;

[TestFixture]
public class CourseRepositoryTest
{
    [SetUp]
    public void Setup()
    {
        _mockSql = new Mock<ISqlExecutor>();
        _mockLogger = new Mock<ILogger<ICourseRepository>>();
        _courseRepository = new CourseRepository(_mockSql.Object, _mockLogger.Object);
    }

    private Mock<ISqlExecutor> _mockSql;
    private Mock<ILogger<ICourseRepository>> _mockLogger;
    private CourseRepository _courseRepository;

    [Test]
    public async Task GetAll_WhenCoursesExist_ShouldReturnCourses()
    {
        // Arrange
        var expectedCourses = new List<Course>
        {
            CreateTestCourse("course-1"),
            CreateTestCourse("course-2")
        };

        _mockSql.Setup(x => x.QueryAsync<Course>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(expectedCourses);

        // Act
        var result = await _courseRepository.GetAll();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].Id, Is.EqualTo("course-1"));
        Assert.That(result[1].Id, Is.EqualTo("course-2"));

        _mockSql.Verify(x => x.QueryAsync<Course>(
                It.IsAny<string>(),
                It.IsAny<object>()),
            Times.Once);
    }

    [Test]
    public async Task GetAll_WhenNoCoursesExist_ShouldReturnEmptyList()
    {
        // Arrange
        _mockSql.Setup(x => x.QueryAsync<Course>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(new List<Course>());

        // Act
        var result = await _courseRepository.GetAll();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0));

        _mockSql.Verify(x => x.QueryAsync<Course>(
                It.IsAny<string>(),
                It.IsAny<object>()),
            Times.Once);
    }

    [Test]
    public Task GetAll_WhenExceptionOccurs_ShouldLogAndRethrow()
    {
        // Arrange
        var exception = new Exception("Database error");

        _mockSql.Setup(x => x.QueryAsync<Course>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _courseRepository.GetAll());
        Assert.That(ex.Message, Does.Contain("Error retrieving all courses"));

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
    public async Task GetById_WhenCourseExists_ShouldReturnCourse()
    {
        // Arrange
        var courseId = "course-123";
        var expectedCourse = CreateTestCourse(courseId);

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<Course>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(expectedCourse);

        // Act
        var result = await _courseRepository.GetById(courseId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(courseId));
        Assert.That(result.Title, Is.EqualTo($"Test Course {courseId}"));

        _mockSql.Verify(x => x.QueryFirstOrDefaultAsync<Course>(
                It.IsAny<string>(),
                It.Is<object>(p => p.GetType().GetProperty("CourseId")!.GetValue(p)!.ToString() == courseId)),
            Times.Once);
    }

    [Test]
    public async Task GetById_WhenCourseDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var courseId = "course-123";

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<Course>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync((Course)null!);

        // Act
        var result = await _courseRepository.GetById(courseId);

        // Assert
        Assert.That(result, Is.Null);

        _mockSql.Verify(x => x.QueryFirstOrDefaultAsync<Course>(
                It.IsAny<string>(),
                It.Is<object>(p => p.GetType().GetProperty("CourseId")!.GetValue(p)!.ToString() == courseId)),
            Times.Once);
    }

    [Test]
    public Task GetById_WhenExceptionOccurs_ShouldLogAndRethrow()
    {
        // Arrange
        var courseId = "course-123";
        var exception = new Exception("Database error");

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<Course>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _courseRepository.GetById(courseId));
        Assert.That(ex.Message, Does.Contain("Error retrieving course by ID"));

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
    public async Task GetBySlug_WhenCourseExists_ShouldReturnCourse()
    {
        // Arrange
        var slug = "course-slug";
        var expectedCourse = CreateTestCourse("course-123", slug);

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<Course>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(expectedCourse);

        // Act
        var result = await _courseRepository.GetBySlug(slug);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Slug, Is.EqualTo(slug));
        Assert.That(result.Id, Is.EqualTo("course-123"));

        _mockSql.Verify(x => x.QueryFirstOrDefaultAsync<Course>(
                It.IsAny<string>(),
                It.Is<object>(p => p.GetType().GetProperty("Slug")!.GetValue(p)!.ToString() == slug)),
            Times.Once);
    }

    [Test]
    public async Task GetBySlug_WhenCourseDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var slug = "course-slug";

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<Course>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync((Course)null!);

        // Act
        var result = await _courseRepository.GetBySlug(slug);

        // Assert
        Assert.That(result, Is.Null);

        _mockSql.Verify(x => x.QueryFirstOrDefaultAsync<Course>(
                It.IsAny<string>(),
                It.Is<object>(p => p.GetType().GetProperty("Slug")!.GetValue(p)!.ToString() == slug)),
            Times.Once);
    }

    [Test]
    public Task GetBySlug_WhenExceptionOccurs_ShouldLogAndRethrow()
    {
        // Arrange
        var slug = "course-slug";
        var exception = new Exception("Database error");

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<Course>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _courseRepository.GetBySlug(slug));
        Assert.That(ex.Message, Does.Contain("Error retrieving course by slug"));

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
    public async Task GetByLanguage_WhenCourseExists_ShouldReturnCourse()
    {
        // Arrange
        var language = "csharp";
        var expectedCourse = CreateTestCourse("course-123", "course-slug", language);

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<Course>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(expectedCourse);

        // Act
        var result = await _courseRepository.GetByLanguage(language);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Language, Is.EqualTo(language));
        Assert.That(result.Id, Is.EqualTo("course-123"));

        _mockSql.Verify(x => x.QueryFirstOrDefaultAsync<Course>(
                It.IsAny<string>(),
                It.Is<object>(p => p.GetType().GetProperty("Language")!.GetValue(p)!.ToString() == language)),
            Times.Once);
    }

    [Test]
    public async Task GetByLanguage_WhenCourseDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var language = "csharp";

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<Course>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync((Course)null!);

        // Act
        var result = await _courseRepository.GetByLanguage(language);

        // Assert
        Assert.That(result, Is.Null);

        _mockSql.Verify(x => x.QueryFirstOrDefaultAsync<Course>(
                It.IsAny<string>(),
                It.Is<object>(p => p.GetType().GetProperty("Language")!.GetValue(p)!.ToString() == language)),
            Times.Once);
    }

    [Test]
    public Task GetByLanguage_WhenExceptionOccurs_ShouldLogAndRethrow()
    {
        // Arrange
        var language = "csharp";
        var exception = new Exception("Database error");

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<Course>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _courseRepository.GetByLanguage(language));
        Assert.That(ex.Message, Does.Contain("Error retrieving course by language"));

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
    public async Task GetByTag_WhenCoursesExist_ShouldReturnCourses()
    {
        // Arrange
        var tag = "beginner";
        var expectedCourses = new List<Course>
        {
            CreateTestCourse("course-1", "course-1-slug", "csharp", new List<string> { tag }),
            CreateTestCourse("course-2", "course-2-slug", "python", new List<string> { tag })
        };

        _mockSql.Setup(x => x.QueryAsync<Course>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(expectedCourses);

        // Act
        var result = await _courseRepository.GetByTag(tag);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].Id, Is.EqualTo("course-1"));
        Assert.That(result[1].Id, Is.EqualTo("course-2"));
        Assert.That(result[0].Tags, Does.Contain(tag));
        Assert.That(result[1].Tags, Does.Contain(tag));

        _mockSql.Verify(x => x.QueryAsync<Course>(
                It.IsAny<string>(),
                It.Is<object>(p => p.GetType().GetProperty("Tag")!.GetValue(p)!.ToString() == tag)),
            Times.Once);
    }

    [Test]
    public async Task GetByTag_WhenNoCoursesExist_ShouldReturnEmptyList()
    {
        // Arrange
        var tag = "advanced";

        _mockSql.Setup(x => x.QueryAsync<Course>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(new List<Course>());

        // Act
        var result = await _courseRepository.GetByTag(tag);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0));

        _mockSql.Verify(x => x.QueryAsync<Course>(
                It.IsAny<string>(),
                It.Is<object>(p => p.GetType().GetProperty("Tag")!.GetValue(p)!.ToString() == tag)),
            Times.Once);
    }

    [Test]
    public Task GetByTag_WhenExceptionOccurs_ShouldLogAndRethrow()
    {
        // Arrange
        var tag = "beginner";
        var exception = new Exception("Database error");

        _mockSql.Setup(x => x.QueryAsync<Course>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _courseRepository.GetByTag(tag));
        Assert.That(ex.Message, Does.Contain("Error retrieving course by tag"));

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

    private static Course CreateTestCourse(string id, string slug = null, string language = "csharp", List<string> tags = null)
    {
        return new Course
        {
            Id = id,
            Title = $"Test Course {id}",
            Slug = slug ?? $"test-course-{id}",
            Description = "Test description",
            Language = language,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Tags = tags ?? new List<string> { "test" },
            FeaturedImage = "https://example.com/image.jpg",
            LessonSummaries = new List<LessonSummary>(),
            Status = "published"
        };
    }
}