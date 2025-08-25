using AscendDev.Core.TestsExecution.KeywordValidation;

namespace AscendDev.Core.TestsExecution.KeywordValidation.LanguageAnalyzers;

public class TypeScriptKeywordAnalyzer : BaseLanguageKeywordAnalyzer
{
    public override string Language => "typescript";

    public override string PreprocessCode(string code)
    {
        // Remove single-line comments (//)
        code = RemoveComments(code, "//", "/*", "*/");

        // Remove string literals (both single and double quotes)
        code = RemoveStringLiterals(code, '"');
        code = RemoveStringLiterals(code, '\'');

        // Remove template literals (`...`)
        code = System.Text.RegularExpressions.Regex.Replace(code, @"`(?:[^`\\]|\\.)*`", "``");

        return code;
    }
}