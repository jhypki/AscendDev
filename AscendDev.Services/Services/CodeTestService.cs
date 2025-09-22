using AscendDev.Core.Exceptions;
using AscendDev.Core.Interfaces.CodeExecution;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Interfaces.TestsExecution;
using AscendDev.Core.Models.CodeExecution;
using AscendDev.Core.Models.Courses;
using AscendDev.Core.Models.TestsExecution.KeywordValidation;
using Microsoft.Extensions.Logging;
using System.Security;
using System.Text.Json;

namespace AscendDev.Services.Services;

public class CodeTestService : ICodeTestService
{
    private readonly ITestsExecutor _testsExecutor;
    private readonly ILessonRepository _lessonRepository;
    private readonly ICodeSanitizerFactory _sanitizerFactory;
    private readonly ISubmissionService _submissionService;
    private readonly IUserProgressService _userProgressService;
    private readonly IKeywordValidationService _keywordValidationService;
    private readonly ILogger<CodeTestService> _logger;

    public CodeTestService(
        ITestsExecutor testsExecutor,
        ILessonRepository lessonRepository,
        ICodeSanitizerFactory sanitizerFactory,
        ISubmissionService submissionService,
        IUserProgressService userProgressService,
        IKeywordValidationService keywordValidationService,
        ILogger<CodeTestService> logger)
    {
        _testsExecutor = testsExecutor ?? throw new ArgumentNullException(nameof(testsExecutor));
        _lessonRepository = lessonRepository ?? throw new ArgumentNullException(nameof(lessonRepository));
        _sanitizerFactory = sanitizerFactory ?? throw new ArgumentNullException(nameof(sanitizerFactory));
        _submissionService = submissionService ?? throw new ArgumentNullException(nameof(submissionService));
        _userProgressService = userProgressService ?? throw new ArgumentNullException(nameof(userProgressService));
        _keywordValidationService = keywordValidationService ?? throw new ArgumentNullException(nameof(keywordValidationService));
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

            // Perform keyword validation first if there are keyword requirements
            var keywordValidationResult = await ValidateKeywordsAsync(sanitizedCode, lesson);

            // If keyword validation fails, return early with validation errors
            if (keywordValidationResult != null && !keywordValidationResult.IsValid)
            {
                _logger.LogWarning("Keyword validation failed for lesson {LessonId}", lessonId);
                return new TestResult
                {
                    Success = false,
                    TestResults = new List<TestCaseResult>
                    {
                        new TestCaseResult
                        {
                            Passed = false,
                            TestName = "Keyword Validation",
                            Message = keywordValidationResult.ValidationMessage
                        }
                    },
                    KeywordValidation = keywordValidationResult
                };
            }

            // Execute the tests
            var result = await _testsExecutor.ExecuteAsync(sanitizedCode, lesson);

            // Add keyword validation result to the test result
            result.KeywordValidation = keywordValidationResult;

            // Save submission if we have a user ID (regardless of pass/fail)
            if (userId.HasValue)
            {
                try
                {
                    var submission = new Submission
                    {
                        UserId = userId.Value,
                        LessonId = lessonId,
                        Code = sanitizedCode,
                        Passed = result.Success,
                        SubmittedAt = DateTime.UtcNow,
                        TestResults = JsonSerializer.Serialize(result.TestResults),
                        ExecutionTimeMs = (int)(result.Performance?.ExecutionTimeMs ?? 0),
                        ErrorMessage = result.Success ? null : GetErrorMessage(result)
                    };

                    var submissionId = await _submissionService.CreateSubmissionAsync(submission);
                    _logger.LogInformation("Saved submission for lesson {LessonId} by user {UserId} (Passed: {Passed})",
                        lessonId, userId.Value, result.Success);

                    // If the submission passed, create or update user progress
                    if (result.Success && submissionId > 0)
                    {
                        try
                        {
                            // Check if user has already completed this lesson
                            var hasCompleted = await _userProgressService.HasUserCompletedLessonAsync(userId.Value, lessonId);
                            if (!hasCompleted)
                            {
                                await _userProgressService.MarkLessonAsCompletedAsync(userId.Value, lessonId, submissionId);
                                _logger.LogInformation("Marked lesson {LessonId} as completed for user {UserId} with submission {SubmissionId}",
                                    lessonId, userId.Value, submissionId);
                            }
                        }
                        catch (Exception progressEx)
                        {
                            // Don't fail the test if progress tracking fails
                            _logger.LogError(progressEx, "Failed to update progress for user {UserId} on lesson {LessonId}", userId.Value, lessonId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Don't fail the test if submission saving fails
                    _logger.LogError(ex, "Failed to save submission for user {UserId} on lesson {LessonId}", userId.Value, lessonId);
                }
            }

            return result;
        }
        catch (SecurityException secEx)
        {
            _logger.LogError(secEx, "Security violation in code for lesson {LessonId}", lessonId);
            return new TestResult
            {
                Success = false,
                TestResults = new List<TestCaseResult>
                {
                    new TestCaseResult
                    {
                        Passed = false,
                        TestName = "Code Security Check",
                        Message = $"Security violation: {secEx.Message}"
                    }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running test for lesson {LessonId}", lessonId);
            return new TestResult
            {
                Success = false,
                TestResults = new List<TestCaseResult>
                {
                    new TestCaseResult
                    {
                        Passed = false,
                        TestName = "Test Execution Error",
                        Message = $"An error occurred while running tests: {ex.Message}"
                    }
                }
            };
        }
    }

    private async Task<KeywordValidationResult?> ValidateKeywordsAsync(string code, Lesson lesson)
    {
        try
        {
            if (lesson.TestConfig.KeywordRequirements == null || !lesson.TestConfig.KeywordRequirements.Any())
            {
                return null; // No keyword requirements
            }

            _logger.LogInformation("Performing keyword validation for lesson {LessonId} with {RequirementCount} requirements",
                lesson.Id, lesson.TestConfig.KeywordRequirements.Count);

            return await _keywordValidationService.ValidateKeywordsAsync(
                code,
                lesson.Language,
                lesson.TestConfig.KeywordRequirements);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during keyword validation for lesson {LessonId}", lesson.Id);
            return new KeywordValidationResult
            {
                IsValid = false,
                ValidationMessage = $"Error during keyword validation: {ex.Message}"
            };
        }
    }

    private static string GetErrorMessage(TestResult result)
    {
        if (result.TestResults == null || !result.TestResults.Any())
            return "No test results available";

        var failedTests = result.TestResults.Where(t => !t.Passed).ToList();
        if (!failedTests.Any())
            return null;

        return string.Join("; ", failedTests.Select(t => $"{t.TestName}: {t.Message}"));
    }
}