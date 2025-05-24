using System.Security;
using AscendDev.Core.Interfaces.CodeExecution;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.CodeExecution;
using Microsoft.Extensions.Logging;

namespace AscendDev.Services.Services;

public class CodeExecutionService(
    ICodeExecutor codeExecutor,
    ILogger<CodeExecutionService> logger)
    : ICodeExecutionService
{
    public async Task<CodeExecutionResult> ExecuteCodeAsync(string language, string code)
    {
        try
        {
            var sanitizedCode = SanitizeUserCode(code, language);

            return await codeExecutor.ExecuteAsync(language, sanitizedCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing code for language {Language}", language);
            return new CodeExecutionResult
            {
                Success = false,
                Stdout = string.Empty,
                Stderr = $"Error executing code: {ex.Message}",
                ExitCode = 1
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