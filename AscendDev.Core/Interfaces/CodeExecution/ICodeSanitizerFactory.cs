namespace AscendDev.Core.Interfaces.CodeExecution;

/// <summary>
/// Factory for creating language-specific code sanitizers
/// </summary>
public interface ICodeSanitizerFactory
{
    /// <summary>
    /// Gets the appropriate code sanitizer for the specified language
    /// </summary>
    /// <param name="language">The programming language</param>
    /// <returns>A code sanitizer for the specified language</returns>
    /// <exception cref="System.NotSupportedException">Thrown when the language is not supported</exception>
    ICodeSanitizer GetSanitizer(string language);
}