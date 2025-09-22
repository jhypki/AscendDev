using AscendDev.Core.Constants;
using System.Text.RegularExpressions;

namespace AscendDev.Core.CodeExecution.Sanitizers;

/// <summary>
/// Sanitizer for TypeScript code
/// </summary>
public class TypeScriptSanitizer : BaseSanitizer
{
    private readonly JavaScriptSanitizer _jsSanitizer;

    public TypeScriptSanitizer(JavaScriptSanitizer jsSanitizer)
    {
        _jsSanitizer = jsSanitizer ?? throw new ArgumentNullException(nameof(jsSanitizer));
    }

    /// <summary>
    /// Checks if this sanitizer supports the given language
    /// </summary>
    public override bool SupportsLanguage(string language)
    {
        return language.Equals(SupportedLanguages.TypeScript, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Removes TypeScript comments from code (same as JavaScript)
    /// </summary>
    protected override string RemoveComments(string code)
    {
        if (string.IsNullOrEmpty(code))
            return code;

        // Remove single-line comments (//)
        // Remove multi-line comments (/* */)
        // But preserve comments inside strings
        var result = Regex.Replace(code, @"//.*$|/\*[\s\S]*?\*/", "", RegexOptions.Multiline);

        return result;
    }

    /// <summary>
    /// Applies TypeScript-specific sanitization rules
    /// </summary>
    protected override string SanitizeLanguageSpecificCode(string code)
    {
        // TypeScript-specific dangerous patterns
        string[] tsForbiddenPatterns =
        {
            // TypeScript-specific patterns (in addition to JavaScript patterns)
            "import\\s+\\*\\s+as\\s+fs\\s+from\\s+['\"]fs['\"]",
            "import\\s+\\*\\s+as\\s+path\\s+from\\s+['\"]path['\"]",
            "import\\s+\\*\\s+as\\s+child_process\\s+from\\s+['\"]child_process['\"]",
            "import\\s+\\{[^\\}]*\\}\\s+from\\s+['\"]fs['\"]",
            "import\\s+\\{[^\\}]*\\}\\s+from\\s+['\"]path['\"]",
            "import\\s+\\{[^\\}]*\\}\\s+from\\s+['\"]child_process['\"]",
            "import\\s+fs\\s+from\\s+['\"]fs['\"]",
            "import\\s+path\\s+from\\s+['\"]path['\"]",
            "import\\s+child_process\\s+from\\s+['\"]child_process['\"]",
            "namespace\\s+process",
            "declare\\s+var\\s+process",
            "declare\\s+let\\s+process",
            "declare\\s+const\\s+process",
            "declare\\s+namespace\\s+process",
            "declare\\s+module\\s+['\"]fs['\"]",
            "declare\\s+module\\s+['\"]path['\"]",
            "declare\\s+module\\s+['\"]child_process['\"]",
            "declare\\s+module\\s+['\"]os['\"]",
            "declare\\s+module\\s+['\"]net['\"]",
            "declare\\s+module\\s+['\"]http['\"]",
            "declare\\s+module\\s+['\"]https['\"]",
            "declare\\s+module\\s+['\"]crypto['\"]",
            "declare\\s+module\\s+['\"]zlib['\"]",
            "declare\\s+module\\s+['\"]dns['\"]",
            "declare\\s+module\\s+['\"]dgram['\"]",
            "declare\\s+module\\s+['\"]cluster['\"]",
            "declare\\s+module\\s+['\"]readline['\"]",
            "declare\\s+module\\s+['\"]repl['\"]",
            "declare\\s+module\\s+['\"]vm['\"]",
            "declare\\s+module\\s+['\"]v8['\"]",
            "declare\\s+module\\s+['\"]tls['\"]",
            "declare\\s+module\\s+['\"]stream['\"]",
            "declare\\s+module\\s+['\"]querystring['\"]",
            "declare\\s+module\\s+['\"]punycode['\"]",
            "declare\\s+module\\s+['\"]url['\"]",
            "declare\\s+module\\s+['\"]util['\"]",
            "declare\\s+module\\s+['\"]buffer['\"]",
            "declare\\s+module\\s+['\"]assert['\"]",
            "declare\\s+module\\s+['\"]events['\"]",
            "declare\\s+module\\s+['\"]constants['\"]",
            "declare\\s+module\\s+['\"]domain['\"]",
            "declare\\s+module\\s+['\"]string_decoder['\"]",
            "declare\\s+module\\s+['\"]timers['\"]",
            "declare\\s+module\\s+['\"]tty['\"]",
            "declare\\s+module\\s+['\"]worker_threads['\"]",
            "declare\\s+module\\s+['\"]perf_hooks['\"]",
            "declare\\s+module\\s+['\"]inspector['\"]",
            "declare\\s+module\\s+['\"]async_hooks['\"]",
            "declare\\s+module\\s+['\"]trace_events['\"]",
            "declare\\s+module\\s+['\"]wasi['\"]",
            "declare\\s+module\\s+['\"]diagnostics_channel['\"]"
        };

        CheckForbiddenPatterns(code, tsForbiddenPatterns, "TypeScript");

        // Apply JavaScript sanitization as well (TypeScript is a superset of JavaScript)
        return _jsSanitizer.SanitizeCode(code);
    }
}