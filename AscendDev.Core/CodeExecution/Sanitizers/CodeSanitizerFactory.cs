using AscendDev.Core.Interfaces.CodeExecution;

namespace AscendDev.Core.CodeExecution.Sanitizers;

/// <summary>
/// Factory for creating language-specific code sanitizers
/// </summary>
public class CodeSanitizerFactory : ICodeSanitizerFactory
{
    private readonly IEnumerable<ICodeSanitizer> _sanitizers;

    public CodeSanitizerFactory(IEnumerable<ICodeSanitizer> sanitizers)
    {
        _sanitizers = sanitizers ?? throw new ArgumentNullException(nameof(sanitizers));
    }

    /// <summary>
    /// Gets the appropriate code sanitizer for the specified language
    /// </summary>
    public ICodeSanitizer GetSanitizer(string language)
    {
        if (string.IsNullOrEmpty(language))
            throw new ArgumentException("Language cannot be null or empty", nameof(language));

        var sanitizer = _sanitizers.FirstOrDefault(s => s.SupportsLanguage(language));

        if (sanitizer == null)
            throw new NotSupportedException($"Sanitization for language '{language}' is not supported");

        return sanitizer;
    }
}