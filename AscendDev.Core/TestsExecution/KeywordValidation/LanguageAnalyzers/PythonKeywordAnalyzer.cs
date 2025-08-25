using AscendDev.Core.TestsExecution.KeywordValidation;

namespace AscendDev.Core.TestsExecution.KeywordValidation.LanguageAnalyzers;

public class PythonKeywordAnalyzer : BaseLanguageKeywordAnalyzer
{
    public override string Language => "python";

    public override string PreprocessCode(string code)
    {
        // Remove single-line comments (#)
        code = RemoveComments(code, "#", "", "");

        // Remove string literals (both single and double quotes)
        code = RemoveStringLiterals(code, '"');
        code = RemoveStringLiterals(code, '\'');

        // Remove triple-quoted strings (docstrings)
        code = System.Text.RegularExpressions.Regex.Replace(code, @"'''.*?'''", "\"\"\"\"\"\"", System.Text.RegularExpressions.RegexOptions.Singleline);
        code = System.Text.RegularExpressions.Regex.Replace(code, "\"\"\".*?\"\"\"", "\"\"\"\"\"\"", System.Text.RegularExpressions.RegexOptions.Singleline);

        return code;
    }
}