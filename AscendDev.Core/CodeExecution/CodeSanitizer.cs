using System.Security;
using System.Text.RegularExpressions;
using AscendDev.Core.Constants;

namespace AscendDev.Core.CodeExecution;

/// <summary>
/// Provides language-specific code sanitization to prevent potentially harmful operations
/// </summary>
public static class CodeSanitizer
{
    /// <summary>
    /// Sanitizes user code based on the specified language
    /// </summary>
    /// <param name="code">The user code to sanitize</param>
    /// <param name="language">The programming language of the code</param>
    /// <returns>The sanitized code</returns>
    /// <exception cref="SecurityException">Thrown when potentially harmful code is detected</exception>
    public static string SanitizeCode(string code, string language)
    {
        if (string.IsNullOrEmpty(code))
            throw new ArgumentException("Code cannot be null or empty", nameof(code));

        if (string.IsNullOrEmpty(language))
            throw new ArgumentException("Language cannot be null or empty", nameof(language));

        // Apply common sanitization rules
        SanitizeCommonPatterns(code);

        // Apply language-specific sanitization
        return language.ToLowerInvariant() switch
        {
            SupportedLanguages.Python => SanitizePythonCode(code),
            SupportedLanguages.CSharp => SanitizeCSharpCode(code),
            SupportedLanguages.TypeScript => SanitizeTypeScriptCode(code),
            SupportedLanguages.JavaScript => SanitizeJavaScriptCode(code),
            _ => throw new NotSupportedException($"Sanitization for language '{language}' is not supported")
        };
    }

    private static void SanitizeCommonPatterns(string code)
    {
        // Check for common dangerous patterns across languages
        string[] commonForbiddenPatterns =
        {
            "eval\\s*\\(",
            "exec\\s*\\(",
            "Runtime\\.getRuntime\\(\\)\\.exec",
            "WebAssembly",
            "Deno\\.run",
            "java\\.lang\\.Runtime",
            "process\\.env"
        };

        foreach (var pattern in commonForbiddenPatterns)
        {
            if (Regex.IsMatch(code, pattern, RegexOptions.IgnoreCase))
                throw new SecurityException($"Code contains potentially unsafe operation: {pattern}");
        }
    }

    private static string SanitizePythonCode(string code)
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

        foreach (var pattern in pythonForbiddenPatterns)
        {
            if (Regex.IsMatch(code, pattern, RegexOptions.IgnoreCase))
                throw new SecurityException($"Python code contains potentially unsafe operation: {pattern}");
        }

        return code;
    }

    private static string SanitizeCSharpCode(string code)
    {
        // C#-specific dangerous patterns
        string[] csharpForbiddenPatterns =
        {
            "System\\.Diagnostics\\.Process",
            "System\\.IO\\.File",
            "System\\.IO\\.Directory",
            "System\\.Net\\.WebClient",
            "System\\.Net\\.Http",
            "HttpClient",
            "System\\.Reflection",
            "Assembly",
            "Type\\.GetType",
            "Activator\\.CreateInstance",
            "DllImport",
            "Marshal",
            "IntPtr",
            "UnsafeCode",
            "unsafe\\s+{",
            "fixed\\s+\\(",
            "stackalloc",
            "Environment\\.Exit",
            "Environment\\.GetEnvironmentVariable",
            "Console\\.ReadLine",
            "Console\\.Read",
            "Console\\.ReadKey",
            "Thread\\.Sleep",
            "Task\\.Delay",
            "Parallel",
            "ThreadPool",
            "Mutex",
            "Semaphore",
            "EventWaitHandle",
            "ManualResetEvent",
            "AutoResetEvent",
            "WaitHandle",
            "Monitor",
            "Interlocked",
            "ReaderWriterLock",
            "ReaderWriterLockSlim",
            "SpinLock",
            "SpinWait",
            "Barrier",
            "CountdownEvent",
            "SemaphoreSlim",
            "CancellationToken",
            "CancellationTokenSource",
            "TaskScheduler",
            "TaskFactory",
            "TaskCompletionSource",
            "Lazy",
            "LazyInitializer",
            "ThreadLocal",
            "ThreadStatic",
            "Volatile",
            "MethodImpl",
            "MethodImplOptions",
            "RuntimeHelpers",
            "GCHandle",
            "GC\\.Collect",
            "GC\\.WaitForPendingFinalizers",
            "GC\\.SuppressFinalize",
            "GC\\.ReRegisterForFinalize",
            "GC\\.AddMemoryPressure",
            "GC\\.RemoveMemoryPressure",
            "GC\\.GetTotalMemory",
            "GC\\.GetGeneration",
            "GC\\.GetMaxGeneration",
            "GC\\.KeepAlive",
            "GC\\.RegisterForFullGCNotification",
            "GC\\.CancelFullGCNotification",
            "GC\\.WaitForFullGCApproach",
            "GC\\.WaitForFullGCComplete",
            "GC\\.TryStartNoGCRegion",
            "GC\\.EndNoGCRegion",
            "GC\\.Collect",
            "GC\\.WaitForPendingFinalizers",
            "GC\\.SuppressFinalize",
            "GC\\.ReRegisterForFinalize",
            "GC\\.AddMemoryPressure",
            "GC\\.RemoveMemoryPressure",
            "GC\\.GetTotalMemory",
            "GC\\.GetGeneration",
            "GC\\.GetMaxGeneration",
            "GC\\.KeepAlive",
            "GC\\.RegisterForFullGCNotification",
            "GC\\.CancelFullGCNotification",
            "GC\\.WaitForFullGCApproach",
            "GC\\.WaitForFullGCComplete",
            "GC\\.TryStartNoGCRegion",
            "GC\\.EndNoGCRegion"
        };

        foreach (var pattern in csharpForbiddenPatterns)
        {
            if (Regex.IsMatch(code, pattern, RegexOptions.IgnoreCase))
                throw new SecurityException($"C# code contains potentially unsafe operation: {pattern}");
        }

        return code;
    }

    private static string SanitizeTypeScriptCode(string code)
    {
        // TypeScript-specific dangerous patterns (includes JavaScript patterns)
        return SanitizeJavaScriptCode(code);
    }

    private static string SanitizeJavaScriptCode(string code)
    {
        // JavaScript/TypeScript-specific dangerous patterns
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
            "Function\\s*\\(",
            "setTimeout\\s*\\(",
            "setInterval\\s*\\(",
            "setImmediate\\s*\\(",
            "clearTimeout\\s*\\(",
            "clearInterval\\s*\\(",
            "clearImmediate\\s*\\(",
            "XMLHttpRequest",
            "fetch\\s*\\(",
            "navigator",
            "location",
            "document",
            "window",
            "localStorage",
            "sessionStorage",
            "indexedDB",
            "WebSocket",
            "Worker",
            "SharedWorker",
            "ServiceWorker",
            "Notification",
            "Proxy",
            "Reflect",
            "constructor\\.",
            "constructor\\[",
            "__proto__",
            "prototype",
            "Object\\.create",
            "Object\\.defineProperty",
            "Object\\.defineProperties",
            "Object\\.getOwnPropertyDescriptor",
            "Object\\.getOwnPropertyDescriptors",
            "Object\\.getOwnPropertyNames",
            "Object\\.getOwnPropertySymbols",
            "Object\\.getPrototypeOf",
            "Object\\.setPrototypeOf",
            "Object\\.isExtensible",
            "Object\\.preventExtensions",
            "Object\\.seal",
            "Object\\.isSealed",
            "Object\\.freeze",
            "Object\\.isFrozen"
        };

        foreach (var pattern in jsForbiddenPatterns)
        {
            if (Regex.IsMatch(code, pattern, RegexOptions.IgnoreCase))
                throw new SecurityException($"JavaScript/TypeScript code contains potentially unsafe operation: {pattern}");
        }

        return code;
    }
}