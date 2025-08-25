using AscendDev.Core.Models.TestsExecution.KeywordValidation;

namespace AscendDev.Core.Interfaces.TestsExecution;

public interface IKeywordValidationService
{
    /// <summary>
    /// Validates that the provided code contains all required keywords
    /// </summary>
    /// <param name="code">The user's submitted code</param>
    /// <param name="language">The programming language</param>
    /// <param name="requirements">List of keyword requirements to validate</param>
    /// <returns>Validation result with details about matches and errors</returns>
    Task<KeywordValidationResult> ValidateKeywordsAsync(
        string code,
        string language,
        List<KeywordRequirement> requirements);

    /// <summary>
    /// Gets the appropriate language analyzer for the given language
    /// </summary>
    /// <param name="language">The programming language</param>
    /// <returns>Language-specific keyword analyzer</returns>
    ILanguageKeywordAnalyzer GetAnalyzer(string language);
}