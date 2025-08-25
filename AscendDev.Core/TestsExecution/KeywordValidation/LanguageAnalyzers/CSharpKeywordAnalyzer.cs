using AscendDev.Core.TestsExecution.KeywordValidation;

namespace AscendDev.Core.TestsExecution.KeywordValidation.LanguageAnalyzers;

public class CSharpKeywordAnalyzer : BaseLanguageKeywordAnalyzer
{
    public override string Language => "csharp";

    public override string PreprocessCode(string code)
    {
        // Remove single-line comments (//) and multi-line comments (/* */)
        code = RemoveComments(code, "//", "/*", "*/");

        // Remove string literals
        code = RemoveStringLiterals(code, '"');

        // Remove character literals
        code = RemoveStringLiterals(code, '\'');

        // Remove verbatim strings (@"...")
        code = System.Text.RegularExpressions.Regex.Replace(code, @"@""(?:[^""]|"""")*""", "\"\"");

        return code;
    }
}