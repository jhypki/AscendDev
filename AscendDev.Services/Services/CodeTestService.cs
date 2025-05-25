using AscendDev.Core.Exceptions;
using AscendDev.Core.Interfaces.CodeExecution;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.CodeExecution;
using AscendDev.Core.Models.Courses;
using Microsoft.Extensions.Logging;

namespace AscendDev.Services.Services;

public class CodeTestService : ICodeTestService
{
    private readonly ITestsExecutor _testsExecutor;
    private readonly ILessonRepository _lessonRepository;
    private readonly ICodeSanitizerFactory _sanitizerFactory;
    private readonly IUserProgressService _userProgressService;
    private readonly ILogger<CodeTestService> _logger;

    public CodeTestService(
        ITestsExecutor testsExecutor,
        ILessonRepository lessonRepository,
        ICodeSanitizerFactory sanitizerFactory,
        IUserProgressService userProgressService,
        ILogger<CodeTestService> logger)
    {
        _testsExecutor = testsExecutor ?? throw new ArgumentNullException(nameof(testsExecutor));
        _lessonRepository = lessonRepository ?? throw new ArgumentNullException(nameof(lessonRepository));
        _sanitizerFactory = sanitizerFactory ?? throw new ArgumentNullException(nameof(sanitizerFactory));
        _userProgressService = userProgressService ?? throw new ArgumentNullException(nameof(userProgressService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TestResult> RunTestsAsync(string lessonId, string userCode, Guid? userId = null)
    {
        try
        {
            var lesson = await _lessonRepository.GetById(lessonId);
            if (lesson == null) throw new NotFoundException("Lesson", lessonId);

            // Get the appropriate sanitizer for the language
            var sanitizer = _sanitizerFactory.GetSanitizer(lesson.Language);

            // Sanitize the code
            var sanitizedCode = sanitizer.SanitizeCode(userCode);

            // Execute the tests
            var result = await _testsExecutor.ExecuteAsync(sanitizedCode, lesson);

            // If tests passed and we have a user ID, track the progress
            if (result.Success && userId.HasValue)
            {
                try
                {
                    await _userProgressService.MarkLessonAsCompletedAsync(userId.Value, lessonId, userCode);
                    _logger.LogInformation("Marked lesson {LessonId} as completed for user {UserId}", lessonId, userId.Value);
                }
                catch (Exception ex)
                {
                    // Don't fail the test if progress tracking fails
                    _logger.LogError(ex, "Failed to track progress for user {UserId} on lesson {LessonId}", userId.Value, lessonId);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running test for lesson {LessonId}", lessonId);
            return new TestResult
            {
                Success = false,
                TestResults = new List<TestCaseResult>()
            };
        }
    }
}