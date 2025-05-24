using System.Security;
using System.Text.RegularExpressions;
using AscendDev.Core.Interfaces.CodeExecution;

namespace AscendDev.Core.CodeExecution.Sanitizers;

/// <summary>
/// Base class for language-specific code sanitizers
/// </summary>
public abstract class BaseSanitizer : ICodeSanitizer
{
    /// <summary>
    /// Checks if this sanitizer supports the given language
    /// </summary>
    public abstract bool SupportsLanguage(string language);

    /// <summary>
    /// Sanitizes user code to prevent potentially harmful operations
    /// </summary>
    public string SanitizeCode(string code)
    {
        if (string.IsNullOrEmpty(code))
            throw new ArgumentException("Code cannot be null or empty", nameof(code));

        // Apply common sanitization rules
        SanitizeCommonPatterns(code);

        // Apply language-specific sanitization
        return SanitizeLanguageSpecificCode(code);
    }

    /// <summary>
    /// Applies language-specific sanitization rules
    /// </summary>
    protected abstract string SanitizeLanguageSpecificCode(string code);

    /// <summary>
    /// Checks for common dangerous patterns across languages
    /// </summary>
    protected void SanitizeCommonPatterns(string code)
    {
        string[] commonForbiddenPatterns =
        {
            "eval\\s*\\(",
            "exec\\s*\\(",
            "Runtime\\.getRuntime\\(\\)\\.exec",
            "WebAssembly",
            "Deno\\.run",
            "java\\.lang\\.Runtime",
            "process\\.env"
        };

        foreach (var pattern in commonForbiddenPatterns)
        {
            if (Regex.IsMatch(code, pattern, RegexOptions.IgnoreCase))
                throw new SecurityException($"Code contains potentially unsafe operation: {pattern}");
        }
    }

    /// <summary>
    /// Checks if code contains any of the specified forbidden patterns
    /// </summary>
    protected void CheckForbiddenPatterns(string code, string[] forbiddenPatterns, string languageName)
    {
        foreach (var pattern in forbiddenPatterns)
        {
            if (Regex.IsMatch(code, pattern, RegexOptions.IgnoreCase))
                throw new SecurityException($"{languageName} code contains potentially unsafe operation: {pattern}");
        }
    }
}