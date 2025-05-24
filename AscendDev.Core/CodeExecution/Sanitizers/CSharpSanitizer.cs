using AscendDev.Core.Constants;

namespace AscendDev.Core.CodeExecution.Sanitizers;

/// <summary>
/// Sanitizer for C# code
/// </summary>
public class CSharpSanitizer : BaseSanitizer
{
    /// <summary>
    /// Checks if this sanitizer supports the given language
    /// </summary>
    public override bool SupportsLanguage(string language)
    {
        return language.Equals(SupportedLanguages.CSharp, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Applies C#-specific sanitization rules
    /// </summary>
    protected override string SanitizeLanguageSpecificCode(string code)
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
            "GC\\.EndNoGCRegion"
        };

        CheckForbiddenPatterns(code, csharpForbiddenPatterns, "C#");

        return code;
    }
}