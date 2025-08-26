using System.Text.RegularExpressions;
using AscendDev.Core.Interfaces.TestsExecution;
using AscendDev.Core.Models.TestsExecution.KeywordValidation;

namespace AscendDev.Core.TestsExecution.KeywordValidation;

public abstract class BaseLanguageKeywordAnalyzer : ILanguageKeywordAnalyzer
{
    public abstract string Language { get; }

    public virtual List<KeywordMatch> FindKeywordMatches(string code, KeywordRequirement requirement)
    {
        var matches = new List<KeywordMatch>();
        var preprocessedCode = PreprocessCode(code);
        var lines = preprocessedCode.Split('\n');

        var regexOptions = requirement.CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
        var escapedKeyword = Regex.Escape(requirement.Keyword);

        string pattern;
        if (requirement.AllowPartialMatch)
        {
            pattern = escapedKeyword;
        }
        else
        {
            // Check if the keyword contains only word characters (letters, digits, underscore)
            // If it does, use word boundaries. Otherwise, use exact match without word boundaries.
            if (Regex.IsMatch(requirement.Keyword, @"^\w+$"))
            {
                pattern = $@"\b{escapedKeyword}\b";
            }
            else
            {
                // For special characters, we need to ensure they're not part of a larger token
                // This is a more conservative approach that looks for the exact sequence
                pattern = escapedKeyword;
            }
        }

        var regex = new Regex(pattern, regexOptions);

        for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            var line = lines[lineIndex];
            var matchCollection = regex.Matches(line);

            foreach (Match match in matchCollection)
            {
                matches.Add(new KeywordMatch
                {
                    Keyword = requirement.Keyword,
                    LineNumber = lineIndex + 1,
                    ColumnStart = match.Index + 1,
                    ColumnEnd = match.Index + match.Length,
                    MatchedText = match.Value
                });
            }
        }

        return matches;
    }

    public virtual KeywordValidationResult ValidateKeyword(string code, KeywordRequirement requirement)
    {
        var result = new KeywordValidationResult();
        var matches = FindKeywordMatches(code, requirement);

        result.Matches.AddRange(matches);
        var occurrences = matches.Count;

        // Check minimum occurrences
        if (occurrences < requirement.MinOccurrences)
        {
            result.IsValid = false;
            result.Errors.Add(new KeywordValidationError
            {
                Keyword = requirement.Keyword,
                ErrorMessage = $"Keyword '{requirement.Keyword}' must appear at least {requirement.MinOccurrences} time(s), but found {occurrences}",
                ErrorType = occurrences == 0 ? KeywordErrorType.Missing : KeywordErrorType.TooFew,
                ExpectedOccurrences = requirement.MinOccurrences,
                ActualOccurrences = occurrences
            });
        }

        // Check maximum occurrences
        if (requirement.MaxOccurrences.HasValue && occurrences > requirement.MaxOccurrences.Value)
        {
            result.IsValid = false;
            result.Errors.Add(new KeywordValidationError
            {
                Keyword = requirement.Keyword,
                ErrorMessage = $"Keyword '{requirement.Keyword}' must appear at most {requirement.MaxOccurrences.Value} time(s), but found {occurrences}",
                ErrorType = KeywordErrorType.TooMany,
                ExpectedOccurrences = requirement.MaxOccurrences.Value,
                ActualOccurrences = occurrences
            });
        }

        if (result.Errors.Count == 0)
        {
            result.IsValid = true;
            result.ValidationMessage = $"Keyword '{requirement.Keyword}' validation passed ({occurrences} occurrence(s) found)";
        }

        return result;
    }

    public abstract string PreprocessCode(string code);

    protected virtual string RemoveComments(string code, string singleLineComment, string multiLineCommentStart, string multiLineCommentEnd)
    {
        // Remove single-line comments
        if (!string.IsNullOrEmpty(singleLineComment))
        {
            var singleLinePattern = $@"{Regex.Escape(singleLineComment)}.*$";
            code = Regex.Replace(code, singleLinePattern, "", RegexOptions.Multiline);
        }

        // Remove multi-line comments
        if (!string.IsNullOrEmpty(multiLineCommentStart) && !string.IsNullOrEmpty(multiLineCommentEnd))
        {
            var multiLinePattern = $@"{Regex.Escape(multiLineCommentStart)}.*?{Regex.Escape(multiLineCommentEnd)}";
            code = Regex.Replace(code, multiLinePattern, "", RegexOptions.Singleline);
        }

        return code;
    }

    protected virtual string RemoveStringLiterals(string code, char stringDelimiter = '"')
    {
        // Simple string removal - this could be enhanced for more complex scenarios
        var pattern = $@"{Regex.Escape(stringDelimiter.ToString())}(?:[^{Regex.Escape(stringDelimiter.ToString())}\\]|\\.)*{Regex.Escape(stringDelimiter.ToString())}";
        return Regex.Replace(code, pattern, "\"\"");
    }
}