using AscendDev.Core.Exceptions;
using AscendDev.Core.Interfaces.CodeExecution;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Interfaces.TestsExecution;
using AscendDev.Core.Models.CodeExecution;
using AscendDev.Core.Models.Courses;
using AscendDev.Core.Models.TestsExecution.KeywordValidation;
using AscendDev.Core.DTOs.CodeExecution;
using Microsoft.Extensions.Logging;

namespace AscendDev.Services.Services;

public class CodeTestService : ICodeTestService
{
    private readonly ITestsExecutor _testsExecutor;
    private readonly ILessonRepository _lessonRepository;
    private readonly ICodeSanitizerFactory _sanitizerFactory;
    private readonly IUserProgressService _userProgressService;
    private readonly IKeywordValidationService _keywordValidationService;
    private readonly ICodeTemplateService _codeTemplateService;
    private readonly ILogger<CodeTestService> _logger;

    public CodeTestService(
        ITestsExecutor testsExecutor,
        ILessonRepository lessonRepository,
        ICodeSanitizerFactory sanitizerFactory,
        IUserProgressService userProgressService,
        IKeywordValidationService keywordValidationService,
        ICodeTemplateService codeTemplateService,
        ILogger<CodeTestService> logger)
    {
        _testsExecutor = testsExecutor ?? throw new ArgumentNullException(nameof(testsExecutor));
        _lessonRepository = lessonRepository ?? throw new ArgumentNullException(nameof(lessonRepository));
        _sanitizerFactory = sanitizerFactory ?? throw new ArgumentNullException(nameof(sanitizerFactory));
        _userProgressService = userProgressService ?? throw new ArgumentNullException(nameof(userProgressService));
        _keywordValidationService = keywordValidationService ?? throw new ArgumentNullException(nameof(keywordValidationService));
        _codeTemplateService = codeTemplateService ?? throw new ArgumentNullException(nameof(codeTemplateService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TestResult> RunTestsAsync(RunTestsRequest request, Guid? userId = null)
    {
        try
        {
            var lesson = await _lessonRepository.GetById(request.LessonId);
            if (lesson == null) throw new NotFoundException("Lesson", request.LessonId);

            // Validate the request
            if (!request.IsValid)
            {
                return new TestResult
                {
                    Success = false,
                    TestResults = new List<TestCaseResult>
                    {
                        new TestCaseResult
                        {
                            Passed = false,
                            TestName = "Request Validation",
                            Message = "Either Code or EditableRegions must be provided"
                        }
                    }
                };
            }

            string finalCode;

            // Handle template-based submission
            if (request.IsTemplateBasedSubmission)
            {
                // Validate editable regions
                var validationResult = _codeTemplateService.ValidateEditableRegions(lesson.CodeTemplate, request.EditableRegions!);
                if (!validationResult.IsValid)
                {
                    return new TestResult
                    {
                        Success = false,
                        TestResults = new List<TestCaseResult>
                        {
                            new TestCaseResult
                            {
                                Passed = false,
                                TestName = "Template Region Validation",
                                Message = string.Join("; ", validationResult.Errors)
                            }
                        }
                    };
                }

                // Merge template with user code
                finalCode = _codeTemplateService.MergeTemplate(lesson.CodeTemplate, request.EditableRegions!);
            }
            else
            {
                // Direct code submission
                finalCode = request.Code!;
            }

            // Get the appropriate sanitizer for the language
            var sanitizer = _sanitizerFactory.GetSanitizer(lesson.Language);

            // Sanitize the code
            var sanitizedCode = sanitizer.SanitizeCode(finalCode);

            // Perform keyword validation first if there are keyword requirements
            var keywordValidationResult = await ValidateKeywordsAsync(sanitizedCode, lesson);

            // If keyword validation fails, return early with validation errors
            if (keywordValidationResult != null && !keywordValidationResult.IsValid)
            {
                _logger.LogWarning("Keyword validation failed for lesson {LessonId}", request.LessonId);
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

            // If tests passed and we have a user ID, track the progress
            if (result.Success && userId.HasValue)
            {
                try
                {
                    // For template-based submissions, store the original editable regions
                    var codeToStore = request.IsTemplateBasedSubmission ?
                        System.Text.Json.JsonSerializer.Serialize(request.EditableRegions) :
                        finalCode;

                    await _userProgressService.MarkLessonAsCompletedAsync(userId.Value, request.LessonId, codeToStore);
                    _logger.LogInformation("Marked lesson {LessonId} as completed for user {UserId}", request.LessonId, userId.Value);
                }
                catch (Exception ex)
                {
                    // Don't fail the test if progress tracking fails
                    _logger.LogError(ex, "Failed to track progress for user {UserId} on lesson {LessonId}", userId.Value, request.LessonId);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running test for lesson {LessonId}", request.LessonId);
            return new TestResult
            {
                Success = false,
                TestResults = new List<TestCaseResult>()
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
}