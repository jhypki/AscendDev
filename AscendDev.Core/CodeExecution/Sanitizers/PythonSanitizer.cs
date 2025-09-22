using AscendDev.Core.Constants;
using System.Text.RegularExpressions;

namespace AscendDev.Core.CodeExecution.Sanitizers;

/// <summary>
/// Sanitizer for Python code
/// </summary>
public class PythonSanitizer : BaseSanitizer
{
    /// <summary>
    /// Checks if this sanitizer supports the given language
    /// </summary>
    public override bool SupportsLanguage(string language)
    {
        return language.Equals(SupportedLanguages.Python, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Removes Python comments from code
    /// </summary>
    protected override string RemoveComments(string code)
    {
        if (string.IsNullOrEmpty(code))
            return code;

        var lines = code.Split('\n');
        var result = new List<string>();

        foreach (var line in lines)
        {
            var processedLine = line;

            // Remove single-line comments (# comments)
            // But preserve # inside strings
            var inString = false;
            var stringChar = '\0';
            var escaped = false;

            for (int i = 0; i < line.Length; i++)
            {
                var ch = line[i];

                if (escaped)
                {
                    escaped = false;
                    continue;
                }

                if (ch == '\\' && inString)
                {
                    escaped = true;
                    continue;
                }

                if (!inString && (ch == '"' || ch == '\''))
                {
                    inString = true;
                    stringChar = ch;
                }
                else if (inString && ch == stringChar)
                {
                    inString = false;
                    stringChar = '\0';
                }
                else if (!inString && ch == '#')
                {
                    // Found a comment outside of a string
                    processedLine = line.Substring(0, i).TrimEnd();
                    break;
                }
            }

            result.Add(processedLine);
        }

        // Remove multi-line strings/docstrings (triple quotes)
        var codeWithoutComments = string.Join("\n", result);

        // Remove triple-quoted strings (both """ and ''')
        codeWithoutComments = Regex.Replace(codeWithoutComments, @"""""""[\s\S]*?""""""|'''[\s\S]*?'''", "", RegexOptions.Multiline);

        return codeWithoutComments;
    }

    /// <summary>
    /// Applies Python-specific sanitization rules
    /// </summary>
    protected override string SanitizeLanguageSpecificCode(string code)
    {
        // Python-specific dangerous patterns
        string[] pythonForbiddenPatterns =
        {
            "import\\s+os",
            "import\\s+subprocess",
            "import\\s+sys",
            "import\\s+shutil",
            "import\\s+pathlib",
            "__import__\\s*\\(",
            "open\\s*\\(",
            "file\\s*\\(",
            "\\.read\\s*\\(",
            "\\.write\\s*\\(",
            "os\\.system",
            "os\\.popen",
            "os\\.spawn",
            "os\\.exec",
            "subprocess\\.(?:call|Popen|run|check_output)",
            "importlib",
            "ctypes",
            "pty",
            "socket",
            "pickle",
            "marshal",
            "builtins",
            "__builtins__",
            "__class__",
            "__bases__",
            "__subclasses__",
            "__globals__",
            "__code__",
            "__reduce__",
            "__import__",
            "globals\\(\\)",
            "locals\\(\\)",
            "getattr\\s*\\(",
            "setattr\\s*\\(",
            "delattr\\s*\\("
        };

        CheckForbiddenPatterns(code, pythonForbiddenPatterns, "Python");

        return code;
    }
}