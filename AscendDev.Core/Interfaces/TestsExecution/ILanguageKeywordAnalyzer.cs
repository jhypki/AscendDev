using AscendDev.Core.Models.TestsExecution.KeywordValidation;

namespace AscendDev.Core.Interfaces.TestsExecution;

public interface ILanguageKeywordAnalyzer
{
    /// <summary>
    /// The programming language this analyzer supports
    /// </summary>
    string Language { get; }

    /// <summary>
    /// Analyzes code to find keyword matches
    /// </summary>
    /// <param name="code">The source code to analyze</param>
    /// <param name="requirement">The keyword requirement to check</param>
    /// <returns>List of matches found in the code</returns>
    List<KeywordMatch> FindKeywordMatches(string code, KeywordRequirement requirement);

    /// <summary>
    /// Validates a single keyword requirement against the code
    /// </summary>
    /// <param name="code">The source code to validate</param>
    /// <param name="requirement">The keyword requirement to validate</param>
    /// <returns>Validation result for this specific requirement</returns>
    KeywordValidationResult ValidateKeyword(string code, KeywordRequirement requirement);

    /// <summary>
    /// Preprocesses code to remove comments and strings if needed for analysis
    /// </summary>
    /// <param name="code">The raw source code</param>
    /// <returns>Preprocessed code ready for keyword analysis</returns>
    string PreprocessCode(string code);
}