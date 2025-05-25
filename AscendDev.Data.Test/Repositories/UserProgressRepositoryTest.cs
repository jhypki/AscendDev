using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Models.Courses;
using AscendDev.Data.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace AscendDev.Tests.Data.Repositories;

[TestFixture]
public class UserProgressRepositoryTest
{
    [SetUp]
    public void Setup()
    {
        _mockSql = new Mock<ISqlExecutor>();
        _mockLogger = new Mock<ILogger<UserProgressRepository>>();
        _userProgressRepository = new UserProgressRepository(_mockLogger.Object, _mockSql.Object);
    }

    private Mock<ISqlExecutor> _mockSql;
    private Mock<ILogger<UserProgressRepository>> _mockLogger;
    private UserProgressRepository _userProgressRepository;

    [Test]
    public async Task GetByUserId_WhenProgressExists_ShouldReturnProgress()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedProgress = new List<UserProgress>
        {
            CreateTestUserProgress(1, userId, "lesson-1"),
            CreateTestUserProgress(2, userId, "lesson-2")
        };

        _mockSql.Setup(x => x.QueryAsync<UserProgress>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(expectedProgress);

        // Act
        var result = await _userProgressRepository.GetByUserId(userId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].UserId, Is.EqualTo(userId));
        Assert.That(result[1].UserId, Is.EqualTo(userId));
        Assert.That(result[0].LessonId, Is.EqualTo("lesson-1"));
        Assert.That(result[1].LessonId, Is.EqualTo("lesson-2"));

        _mockSql.Verify(x => x.QueryAsync<UserProgress>(
                It.IsAny<string>(),
                It.Is<object>(p => p.GetType().GetProperty("UserId")!.GetValue(p)!.ToString() == userId.ToString())),
            Times.Once);
    }

    [Test]
    public async Task GetByUserId_WhenNoProgressExists_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockSql.Setup(x => x.QueryAsync<UserProgress>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(new List<UserProgress>());

        // Act
        var result = await _userProgressRepository.GetByUserId(userId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0));

        _mockSql.Verify(x => x.QueryAsync<UserProgress>(
                It.IsAny<string>(),
                It.Is<object>(p => p.GetType().GetProperty("UserId")!.GetValue(p)!.ToString() == userId.ToString())),
            Times.Once);
    }

    [Test]
    public Task GetByUserId_WhenExceptionOccurs_ShouldLogAndRethrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exception = new Exception("Database error");

        _mockSql.Setup(x => x.QueryAsync<UserProgress>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _userProgressRepository.GetByUserId(userId));
        Assert.That(ex.Message, Does.Contain("Error retrieving progress for user"));

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
    public async Task GetByLessonId_WhenProgressExists_ShouldReturnProgress()
    {
        // Arrange
        var lessonId = "lesson-123";
        var expectedProgress = new List<UserProgress>
        {
            CreateTestUserProgress(1, Guid.NewGuid(), lessonId),
            CreateTestUserProgress(2, Guid.NewGuid(), lessonId)
        };

        _mockSql.Setup(x => x.QueryAsync<UserProgress>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(expectedProgress);

        // Act
        var result = await _userProgressRepository.GetByLessonId(lessonId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].LessonId, Is.EqualTo(lessonId));
        Assert.That(result[1].LessonId, Is.EqualTo(lessonId));

        _mockSql.Verify(x => x.QueryAsync<UserProgress>(
                It.IsAny<string>(),
                It.Is<object>(p => p.GetType().GetProperty("LessonId")!.GetValue(p)!.ToString() == lessonId)),
            Times.Once);
    }

    [Test]
    public async Task GetByLessonId_WhenNoProgressExists_ShouldReturnEmptyList()
    {
        // Arrange
        var lessonId = "lesson-123";

        _mockSql.Setup(x => x.QueryAsync<UserProgress>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(new List<UserProgress>());

        // Act
        var result = await _userProgressRepository.GetByLessonId(lessonId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0));

        _mockSql.Verify(x => x.QueryAsync<UserProgress>(
                It.IsAny<string>(),
                It.Is<object>(p => p.GetType().GetProperty("LessonId")!.GetValue(p)!.ToString() == lessonId)),
            Times.Once);
    }

    [Test]
    public Task GetByLessonId_WhenExceptionOccurs_ShouldLogAndRethrow()
    {
        // Arrange
        var lessonId = "lesson-123";
        var exception = new Exception("Database error");

        _mockSql.Setup(x => x.QueryAsync<UserProgress>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _userProgressRepository.GetByLessonId(lessonId));
        Assert.That(ex.Message, Does.Contain("Error retrieving progress for lesson"));

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
    public async Task GetByUserAndLessonId_WhenProgressExists_ShouldReturnProgress()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lessonId = "lesson-123";
        var expectedProgress = CreateTestUserProgress(1, userId, lessonId);

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<UserProgress>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(expectedProgress);

        // Act
        var result = await _userProgressRepository.GetByUserAndLessonId(userId, lessonId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserId, Is.EqualTo(userId));
        Assert.That(result.LessonId, Is.EqualTo(lessonId));

        _mockSql.Verify(x => x.QueryFirstOrDefaultAsync<UserProgress>(
                It.IsAny<string>(),
                It.Is<object>(p =>
                    p.GetType().GetProperty("UserId")!.GetValue(p)!.ToString() == userId.ToString() &&
                    p.GetType().GetProperty("LessonId")!.GetValue(p)!.ToString() == lessonId)),
            Times.Once);
    }

    [Test]
    public async Task GetByUserAndLessonId_WhenProgressDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lessonId = "lesson-123";

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<UserProgress>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync((UserProgress)null!);

        // Act
        var result = await _userProgressRepository.GetByUserAndLessonId(userId, lessonId);

        // Assert
        Assert.That(result, Is.Null);

        _mockSql.Verify(x => x.QueryFirstOrDefaultAsync<UserProgress>(
                It.IsAny<string>(),
                It.Is<object>(p =>
                    p.GetType().GetProperty("UserId")!.GetValue(p)!.ToString() == userId.ToString() &&
                    p.GetType().GetProperty("LessonId")!.GetValue(p)!.ToString() == lessonId)),
            Times.Once);
    }

    [Test]
    public Task GetByUserAndLessonId_WhenExceptionOccurs_ShouldLogAndRethrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lessonId = "lesson-123";
        var exception = new Exception("Database error");

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<UserProgress>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _userProgressRepository.GetByUserAndLessonId(userId, lessonId));
        Assert.That(ex.Message, Does.Contain("Error retrieving progress for user"));

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
    public async Task Create_WhenSuccessful_ShouldReturnCreatedProgress()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lessonId = "lesson-123";
        var progress = CreateTestUserProgress(0, userId, lessonId); // ID will be assigned by DB
        var createdProgress = CreateTestUserProgress(1, userId, lessonId);

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<UserProgress>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(createdProgress);

        // Act
        var result = await _userProgressRepository.Create(progress);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.UserId, Is.EqualTo(userId));
        Assert.That(result.LessonId, Is.EqualTo(lessonId));

        _mockSql.Verify(x => x.QueryFirstOrDefaultAsync<UserProgress>(
                It.IsAny<string>(),
                It.Is<object>(p =>
                    p.GetType().GetProperty("UserId")!.GetValue(p)!.ToString() == userId.ToString() &&
                    p.GetType().GetProperty("LessonId")!.GetValue(p)!.ToString() == lessonId)),
            Times.Once);
    }

    [Test]
    public Task Create_WhenExceptionOccurs_ShouldLogAndRethrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lessonId = "lesson-123";
        var progress = CreateTestUserProgress(0, userId, lessonId);
        var exception = new Exception("Database error");

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<UserProgress>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _userProgressRepository.Create(progress));
        Assert.That(ex.Message, Does.Contain("Error creating progress for user"));

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
    public async Task Update_WhenSuccessful_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lessonId = "lesson-123";
        var progress = CreateTestUserProgress(1, userId, lessonId);

        _mockSql.Setup(x => x.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(1);

        // Act
        var result = await _userProgressRepository.Update(progress);

        // Assert
        Assert.That(result, Is.True);

        _mockSql.Verify(x => x.ExecuteAsync(
                It.IsAny<string>(),
                It.Is<object>(p => p.GetType().GetProperty("Id")!.GetValue(p)!.ToString() == "1")),
            Times.Once);
    }

    [Test]
    public async Task Update_WhenNoRowsAffected_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lessonId = "lesson-123";
        var progress = CreateTestUserProgress(1, userId, lessonId);

        _mockSql.Setup(x => x.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(0);

        // Act
        var result = await _userProgressRepository.Update(progress);

        // Assert
        Assert.That(result, Is.False);

        _mockSql.Verify(x => x.ExecuteAsync(
                It.IsAny<string>(),
                It.Is<object>(p => p.GetType().GetProperty("Id")!.GetValue(p)!.ToString() == "1")),
            Times.Once);
    }

    [Test]
    public Task Update_WhenExceptionOccurs_ShouldLogAndRethrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lessonId = "lesson-123";
        var progress = CreateTestUserProgress(1, userId, lessonId);
        var exception = new Exception("Database error");

        _mockSql.Setup(x => x.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _userProgressRepository.Update(progress));
        Assert.That(ex.Message, Does.Contain("Error updating progress with ID"));

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
    public async Task Delete_WhenSuccessful_ShouldReturnTrue()
    {
        // Arrange
        var progressId = 1;

        _mockSql.Setup(x => x.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(1);

        // Act
        var result = await _userProgressRepository.Delete(progressId);

        // Assert
        Assert.That(result, Is.True);

        _mockSql.Verify(x => x.ExecuteAsync(
                It.IsAny<string>(),
                It.Is<object>(p => p.GetType().GetProperty("Id")!.GetValue(p)!.ToString() == progressId.ToString())),
            Times.Once);
    }

    [Test]
    public async Task Delete_WhenNoRowsAffected_ShouldReturnFalse()
    {
        // Arrange
        var progressId = 1;

        _mockSql.Setup(x => x.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(0);

        // Act
        var result = await _userProgressRepository.Delete(progressId);

        // Assert
        Assert.That(result, Is.False);

        _mockSql.Verify(x => x.ExecuteAsync(
                It.IsAny<string>(),
                It.Is<object>(p => p.GetType().GetProperty("Id")!.GetValue(p)!.ToString() == progressId.ToString())),
            Times.Once);
    }

    [Test]
    public Task Delete_WhenExceptionOccurs_ShouldLogAndRethrow()
    {
        // Arrange
        var progressId = 1;
        var exception = new Exception("Database error");

        _mockSql.Setup(x => x.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _userProgressRepository.Delete(progressId));
        Assert.That(ex.Message, Does.Contain("Error deleting progress with ID"));

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
    public async Task HasUserCompletedLesson_WhenLessonCompleted_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lessonId = "lesson-123";

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<int>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(1);

        // Act
        var result = await _userProgressRepository.HasUserCompletedLesson(userId, lessonId);

        // Assert
        Assert.That(result, Is.True);

        _mockSql.Verify(x => x.QueryFirstOrDefaultAsync<int>(
                It.IsAny<string>(),
                It.Is<object>(p =>
                    p.GetType().GetProperty("UserId")!.GetValue(p)!.ToString() == userId.ToString() &&
                    p.GetType().GetProperty("LessonId")!.GetValue(p)!.ToString() == lessonId)),
            Times.Once);
    }

    [Test]
    public async Task HasUserCompletedLesson_WhenLessonNotCompleted_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lessonId = "lesson-123";

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<int>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(0);

        // Act
        var result = await _userProgressRepository.HasUserCompletedLesson(userId, lessonId);

        // Assert
        Assert.That(result, Is.False);

        _mockSql.Verify(x => x.QueryFirstOrDefaultAsync<int>(
                It.IsAny<string>(),
                It.Is<object>(p =>
                    p.GetType().GetProperty("UserId")!.GetValue(p)!.ToString() == userId.ToString() &&
                    p.GetType().GetProperty("LessonId")!.GetValue(p)!.ToString() == lessonId)),
            Times.Once);
    }

    [Test]
    public Task HasUserCompletedLesson_WhenExceptionOccurs_ShouldLogAndRethrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lessonId = "lesson-123";
        var exception = new Exception("Database error");

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<int>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _userProgressRepository.HasUserCompletedLesson(userId, lessonId));
        Assert.That(ex.Message, Does.Contain("Error checking if user"));

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
    public async Task GetCompletedLessonCountForCourse_WhenLessonsCompleted_ShouldReturnCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var courseId = "course-123";
        var expectedCount = 5;

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<int>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _userProgressRepository.GetCompletedLessonCountForCourse(userId, courseId);

        // Assert
        Assert.That(result, Is.EqualTo(expectedCount));

        _mockSql.Verify(x => x.QueryFirstOrDefaultAsync<int>(
                It.IsAny<string>(),
                It.Is<object>(p =>
                    p.GetType().GetProperty("UserId")!.GetValue(p)!.ToString() == userId.ToString() &&
                    p.GetType().GetProperty("CourseId")!.GetValue(p)!.ToString() == courseId)),
            Times.Once);
    }

    [Test]
    public Task GetCompletedLessonCountForCourse_WhenExceptionOccurs_ShouldLogAndRethrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var courseId = "course-123";
        var exception = new Exception("Database error");

        _mockSql.Setup(x => x.QueryFirstOrDefaultAsync<int>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _userProgressRepository.GetCompletedLessonCountForCourse(userId, courseId));
        Assert.That(ex.Message, Does.Contain("Error getting completed lesson count"));

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

    private static UserProgress CreateTestUserProgress(int id, Guid userId, string lessonId)
    {
        return new UserProgress
        {
            Id = id,
            UserId = userId,
            LessonId = lessonId,
            CompletedAt = DateTime.UtcNow,
            CodeSolution = "public class Solution { }"
        };
    }
}