using AscendDev.Core.TestsExecution.KeywordValidation;

namespace AscendDev.Core.TestsExecution.KeywordValidation.LanguageAnalyzers;

public class GoKeywordAnalyzer : BaseLanguageKeywordAnalyzer
{
    public override string Language => "go";

    public override string PreprocessCode(string code)
    {
        // Remove single-line comments (//)
        code = RemoveComments(code, "//", "", "");

        // Remove multi-line comments (/* */)
        code = RemoveComments(code, "/*", "*/", "");

        // Remove string literals (both single and double quotes)
        code = RemoveStringLiterals(code, '"');
        code = RemoveStringLiterals(code, '\'');

        // Remove raw string literals (`backticks`)
        code = System.Text.RegularExpressions.Regex.Replace(code, @"`[^`]*`", "\"\"", System.Text.RegularExpressions.RegexOptions.Singleline);

        return code;
    }
}