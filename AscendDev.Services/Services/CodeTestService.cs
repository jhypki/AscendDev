using System.Security;
using AscendDev.Core.Exceptions;
using AscendDev.Core.Interfaces.CodeExecution;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.CodeExecution;
using Microsoft.Extensions.Logging;

namespace AscendDev.Services.Services;

public class CodeTestService(
    ITestsExecutor testsExecutor,
    ILessonRepository lessonRepository,
    ILogger<CodeTestService> logger)
    : ICodeTestService
{
    public async Task<TestResult> RunTestsAsync(string lessonId, string userCode)
    {
        try
        {
            var lesson = await lessonRepository.GetById(lessonId);
            if (lesson == null) throw new NotFoundException("Lesson", lessonId);

            var sanitizedCode = SanitizeUserCode(userCode, lesson.Language);

            return await testsExecutor.ExecuteAsync(sanitizedCode, lesson);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error running test for lesson {LessonId}", lessonId);
            return new TestResult
            {
                Success = false,
                TestResults = new List<TestCaseResult>()
            };
        }
    }

    private string SanitizeUserCode(string userCode, string language)
    {
        //TODO: Implement a more robust sanitization process for each language

        string[] forbiddenPatterns =
        {
            "System.Diagnostics.Process",
            "eval(",
            "exec(",
            "os.system",
            "subprocess",
            "Runtime.getRuntime().exec",
            "System.IO.File",
            "fs.readFileSync",
            "fs.writeFileSync",
            "require('child_process')",
            "import('child_process')",
            "new Function(",
            "Function(",
            "WebAssembly",
            "Deno.run",
            "java.lang.Runtime",
            "System.Net.WebClient",
            "System.Net.Http",
            "HttpClient",
            "__dirname",
            "__filename",
            "process.env"
        };

        foreach (var pattern in forbiddenPatterns)
            if (userCode.Contains(pattern))
                throw new SecurityException($"Code contains potentially unsafe operation: {pattern}");

        return userCode;
    }
}