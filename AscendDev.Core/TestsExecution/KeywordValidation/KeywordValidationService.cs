using AscendDev.Core.Interfaces.TestsExecution;
using AscendDev.Core.Models.TestsExecution.KeywordValidation;
using AscendDev.Core.TestsExecution.KeywordValidation.LanguageAnalyzers;
using Microsoft.Extensions.Logging;

namespace AscendDev.Core.TestsExecution.KeywordValidation;

public class KeywordValidationService : IKeywordValidationService
{
    private readonly Dictionary<string, ILanguageKeywordAnalyzer> _analyzers;
    private readonly ILogger<KeywordValidationService> _logger;

    public KeywordValidationService(ILogger<KeywordValidationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _analyzers = new Dictionary<string, ILanguageKeywordAnalyzer>(StringComparer.OrdinalIgnoreCase)
        {
            { "go", new GoKeywordAnalyzer() },
            { "python", new PythonKeywordAnalyzer() },
            { "typescript", new TypeScriptKeywordAnalyzer() },
            { "javascript", new TypeScriptKeywordAnalyzer() } // JavaScript uses same analyzer as TypeScript
        };
    }

    public async Task<KeywordValidationResult> ValidateKeywordsAsync(
        string code,
        string language,
        List<KeywordRequirement> requirements)
    {
        try
        {
            var analyzer = GetAnalyzer(language);
            var overallResult = new KeywordValidationResult { IsValid = true };

            foreach (var requirement in requirements)
            {
                if (!requirement.Required)
                    continue;

                var keywordResult = analyzer.ValidateKeyword(code, requirement);

                // Merge results
                overallResult.Matches.AddRange(keywordResult.Matches);
                overallResult.Errors.AddRange(keywordResult.Errors);

                if (!keywordResult.IsValid)
                {
                    overallResult.IsValid = false;
                }
            }

            // Set overall validation message
            if (overallResult.IsValid)
            {
                overallResult.ValidationMessage = $"All required keywords found. Validation passed.";
                _logger.LogInformation("Keyword validation passed for {Language} code", language);
            }
            else
            {
                overallResult.ValidationMessage = $"Keyword validation failed. {overallResult.Errors.Count} error(s) found.";
                _logger.LogWarning("Keyword validation failed for {Language} code with {ErrorCount} errors",
                    language, overallResult.Errors.Count);
            }

            return overallResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during keyword validation for {Language}", language);
            return new KeywordValidationResult
            {
                IsValid = false,
                ValidationMessage = $"Error during keyword validation: {ex.Message}",
                Errors = new List<KeywordValidationError>
                {
                    new KeywordValidationError
                    {
                        Keyword = "SYSTEM_ERROR",
                        ErrorMessage = $"System error during validation: {ex.Message}",
                        ErrorType = KeywordErrorType.Missing
                    }
                }
            };
        }
    }

    public ILanguageKeywordAnalyzer GetAnalyzer(string language)
    {
        if (_analyzers.TryGetValue(language, out var analyzer))
        {
            return analyzer;
        }

        throw new NotSupportedException($"Language '{language}' is not supported for keyword validation. Supported languages: {string.Join(", ", _analyzers.Keys)}");
    }
}