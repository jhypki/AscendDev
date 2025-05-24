using AscendDev.Core.Interfaces.CodeExecution;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.CodeExecution;
using Microsoft.Extensions.Logging;

namespace AscendDev.Services.Services;

public class CodeExecutionService : ICodeExecutionService
{
    private readonly ICodeExecutor _codeExecutor;
    private readonly ICodeSanitizerFactory _sanitizerFactory;
    private readonly ILogger<CodeExecutionService> _logger;

    public CodeExecutionService(
        ICodeExecutor codeExecutor,
        ICodeSanitizerFactory sanitizerFactory,
        ILogger<CodeExecutionService> logger)
    {
        _codeExecutor = codeExecutor ?? throw new ArgumentNullException(nameof(codeExecutor));
        _sanitizerFactory = sanitizerFactory ?? throw new ArgumentNullException(nameof(sanitizerFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CodeExecutionResult> ExecuteCodeAsync(string language, string code)
    {
        try
        {
            // Get the appropriate sanitizer for the language
            var sanitizer = _sanitizerFactory.GetSanitizer(language);

            // Sanitize the code
            var sanitizedCode = sanitizer.SanitizeCode(code);

            // Execute the sanitized code
            return await _codeExecutor.ExecuteAsync(language, sanitizedCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing code for language {Language}", language);
            return new CodeExecutionResult
            {
                Success = false,
                Stdout = string.Empty,
                Stderr = $"Error executing code: {ex.Message}",
                ExitCode = 1
            };
        }
    }
}