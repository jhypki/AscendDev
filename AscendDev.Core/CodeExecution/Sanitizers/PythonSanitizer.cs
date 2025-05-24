using AscendDev.Core.Constants;

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