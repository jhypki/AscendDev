using AscendDev.Core.Constants;

namespace AscendDev.Core.CodeExecution.Sanitizers;

/// <summary>
/// Sanitizer for JavaScript code
/// </summary>
public class JavaScriptSanitizer : BaseSanitizer
{
    /// <summary>
    /// Checks if this sanitizer supports the given language
    /// </summary>
    public override bool SupportsLanguage(string language)
    {
        return language.Equals(SupportedLanguages.JavaScript, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Applies JavaScript-specific sanitization rules
    /// </summary>
    protected override string SanitizeLanguageSpecificCode(string code)
    {
        // JavaScript-specific dangerous patterns
        string[] jsForbiddenPatterns =
        {
            "require\\s*\\(",
            "import\\s*\\(",
            "process",
            "child_process",
            "fs\\.",
            "fs\\[",
            "path\\.",
            "path\\[",
            "net\\.",
            "net\\[",
            "http\\.",
            "http\\[",
            "https\\.",
            "https\\[",
            "os\\.",
            "os\\[",
            "crypto\\.",
            "crypto\\[",
            "zlib\\.",
            "zlib\\[",
            "dns\\.",
            "dns\\[",
            "dgram\\.",
            "dgram\\[",
            "cluster\\.",
            "cluster\\[",
            "readline\\.",
            "readline\\[",
            "repl\\.",
            "repl\\[",
            "vm\\.",
            "vm\\[",
            "v8\\.",
            "v8\\[",
            "tls\\.",
            "tls\\[",
            "stream\\.",
            "stream\\[",
            "querystring\\.",
            "querystring\\[",
            "punycode\\.",
            "punycode\\[",
            "url\\.",
            "url\\[",
            "util\\.",
            "util\\[",
            "buffer\\.",
            "buffer\\[",
            "assert\\.",
            "assert\\[",
            "events\\.",
            "events\\[",
            "constants\\.",
            "constants\\[",
            "domain\\.",
            "domain\\[",
            "string_decoder\\.",
            "string_decoder\\[",
            "timers\\.",
            "timers\\[",
            "tty\\.",
            "tty\\[",
            "worker_threads\\.",
            "worker_threads\\[",
            "perf_hooks\\.",
            "perf_hooks\\[",
            "inspector\\.",
            "inspector\\[",
            "async_hooks\\.",
            "async_hooks\\[",
            "trace_events\\.",
            "trace_events\\[",
            "wasi\\.",
            "wasi\\[",
            "diagnostics_channel\\.",
            "diagnostics_channel\\[",
            "performance\\.",
            "performance\\[",
            "module\\.",
            "module\\[",
            "__dirname",
            "__filename",
            "new\\s+Function\\s*\\(",
            "Function\\s*\\("
        };

        CheckForbiddenPatterns(code, jsForbiddenPatterns, "JavaScript");

        return code;
    }
}