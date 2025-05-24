namespace AscendDev.Core.Interfaces.CodeExecution;

/// <summary>
/// Interface for language-specific code sanitization
/// </summary>
public interface ICodeSanitizer
{
    /// <summary>
    /// Checks if this sanitizer supports the given language
    /// </summary>
    bool SupportsLanguage(string language);

    /// <summary>
    /// Sanitizes user code to prevent potentially harmful operations
    /// </summary>
    /// <param name="code">The user code to sanitize</param>
    /// <returns>The sanitized code</returns>
    /// <exception cref="System.Security.SecurityException">Thrown when potentially harmful code is detected</exception>
    string SanitizeCode(string code);
}