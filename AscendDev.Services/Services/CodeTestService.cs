using AscendDev.Core.Exceptions;
using AscendDev.Core.Interfaces.CodeExecution;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.CodeExecution;
using Microsoft.Extensions.Logging;

namespace AscendDev.Services.Services;

public class CodeTestService : ICodeTestService
{
    private readonly ITestsExecutor _testsExecutor;
    private readonly ILessonRepository _lessonRepository;
    private readonly ICodeSanitizerFactory _sanitizerFactory;
    private readonly ILogger<CodeTestService> _logger;

    public CodeTestService(
        ITestsExecutor testsExecutor,
        ILessonRepository lessonRepository,
        ICodeSanitizerFactory sanitizerFactory,
        ILogger<CodeTestService> logger)
    {
        _testsExecutor = testsExecutor ?? throw new ArgumentNullException(nameof(testsExecutor));
        _lessonRepository = lessonRepository ?? throw new ArgumentNullException(nameof(lessonRepository));
        _sanitizerFactory = sanitizerFactory ?? throw new ArgumentNullException(nameof(sanitizerFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TestResult> RunTestsAsync(string lessonId, string userCode)
    {
        try
        {
            var lesson = await _lessonRepository.GetById(lessonId);
            if (lesson == null) throw new NotFoundException("Lesson", lessonId);

            // Get the appropriate sanitizer for the language
            var sanitizer = _sanitizerFactory.GetSanitizer(lesson.Language);

            // Sanitize the code
            var sanitizedCode = sanitizer.SanitizeCode(userCode);

            return await _testsExecutor.ExecuteAsync(sanitizedCode, lesson);
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