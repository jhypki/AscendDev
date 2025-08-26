using AscendDev.Core.Constants;
using System.Security;

namespace AscendDev.Core.CodeExecution.Sanitizers;

/// <summary>
/// Sanitizer for Go code
/// </summary>
public class GoSanitizer : BaseSanitizer
{
    /// <summary>
    /// Checks if this sanitizer supports the given language
    /// </summary>
    public override bool SupportsLanguage(string language)
    {
        return language.Equals(SupportedLanguages.Go, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Applies Go-specific sanitization rules
    /// </summary>
    protected override string SanitizeLanguageSpecificCode(string code)
    {
        // Go-specific dangerous patterns
        string[] goForbiddenPatterns =
        {
            // System operations
            @"os\.Exit\s*\(",
            @"os\.Remove\s*\(",
            @"os\.RemoveAll\s*\(",
            @"os\.Rename\s*\(",
            @"os\.Chmod\s*\(",
            @"os\.Chown\s*\(",
            @"os\.Mkdir\s*\(",
            @"os\.MkdirAll\s*\(",
            @"os\.Create\s*\(",
            @"os\.OpenFile\s*\(",
            @"os\.Getenv\s*\(",
            @"os\.Setenv\s*\(",
            @"os\.Unsetenv\s*\(",
            @"os\.Clearenv\s*\(",
            @"os\.Getwd\s*\(",
            @"os\.Chdir\s*\(",
            @"os\.TempDir\s*\(",
            @"os\.UserHomeDir\s*\(",
            @"os\.UserCacheDir\s*\(",
            @"os\.UserConfigDir\s*\(",
            @"os\.Hostname\s*\(",
            @"os\.Getpid\s*\(",
            @"os\.Getppid\s*\(",
            @"os\.Getuid\s*\(",
            @"os\.Geteuid\s*\(",
            @"os\.Getgid\s*\(",
            @"os\.Getegid\s*\(",
            @"os\.Getgroups\s*\(",

            // Process execution
            @"exec\.Command\s*\(",
            @"exec\.CommandContext\s*\(",
            @"exec\.LookPath\s*\(",

            // Network operations
            @"net\.Dial\s*\(",
            @"net\.DialTimeout\s*\(",
            @"net\.Listen\s*\(",
            @"net\.ListenPacket\s*\(",
            @"net\.LookupHost\s*\(",
            @"net\.LookupAddr\s*\(",
            @"net\.LookupCNAME\s*\(",
            @"net\.LookupMX\s*\(",
            @"net\.LookupNS\s*\(",
            @"net\.LookupTXT\s*\(",
            @"http\.Get\s*\(",
            @"http\.Post\s*\(",
            @"http\.PostForm\s*\(",
            @"http\.Head\s*\(",
            @"http\.Client\s*\{",
            @"http\.Transport\s*\{",
            @"http\.ListenAndServe\s*\(",
            @"http\.ListenAndServeTLS\s*\(",
            @"http\.Serve\s*\(",
            @"http\.ServeTLS\s*\(",

            // File system operations
            @"ioutil\.ReadFile\s*\(",
            @"ioutil\.WriteFile\s*\(",
            @"ioutil\.ReadDir\s*\(",
            @"ioutil\.TempFile\s*\(",
            @"ioutil\.TempDir\s*\(",
            @"filepath\.Walk\s*\(",
            @"filepath\.WalkDir\s*\(",

            // Unsafe operations
            @"unsafe\.",
            @"reflect\.ValueOf\s*\(",
            @"reflect\.TypeOf\s*\(",

            // Runtime operations
            @"runtime\.GC\s*\(",
            @"runtime\.GOMAXPROCS\s*\(",
            @"runtime\.Goexit\s*\(",
            @"runtime\.Gosched\s*\(",
            @"runtime\.SetFinalizer\s*\(",
            @"runtime\.Stack\s*\(",

            // CGO operations
            @"import\s+""C""",
            @"\/\*\s*#include",
            @"C\.",

            // Database operations
            @"sql\.Open\s*\(",
            @"database\/sql",

            // Crypto operations that might be used maliciously
            @"crypto\/tls",
            @"crypto\/x509",

            // Plugin loading
            @"plugin\.Open\s*\(",

            // Build constraints that might bypass restrictions
            @"\/\/\s*\+build",
            @"\/\/go:build",

            // Assembly
            @"\/\/go:noescape",
            @"\/\/go:nosplit",
            @"\/\/go:norace",
            @"\/\/go:linkname",

            // Goroutines that might cause resource issues
            @"go\s+func\s*\(",
            @"runtime\.NumGoroutine\s*\(",

            // Channel operations that might cause deadlocks
            @"make\s*\(\s*chan\s+",
            @"<-\s*chan\s+",
            @"chan\s*<-",

            // Time operations that might cause delays
            @"time\.Sleep\s*\(",
            @"time\.After\s*\(",
            @"time\.Tick\s*\(",
            @"time\.NewTicker\s*\(",
            @"time\.NewTimer\s*\(",

            // Context operations
            @"context\.WithCancel\s*\(",
            @"context\.WithDeadline\s*\(",
            @"context\.WithTimeout\s*\(",
            @"context\.WithValue\s*\(",

            // Log operations that might expose information
            @"log\.Fatal\s*\(",
            @"log\.Panic\s*\(",
            @"log\.Print\s*\(",
            @"log\.Printf\s*\(",
            @"log\.Println\s*\(",

            // Testing operations that might interfere
            @"testing\.B\s*\*",
            @"testing\.T\s*\*",
            @"testing\.M\s*\*"
        };

        CheckForbiddenPatterns(code, goForbiddenPatterns, "Go");

        // Check for dangerous imports
        CheckDangerousImports(code);

        // Return the original code if no issues found
        return code;
    }

    /// <summary>
    /// Checks for dangerous import statements
    /// </summary>
    private void CheckDangerousImports(string code)
    {
        string[] dangerousImports =
        {
            "\"os\"",
            "\"os/exec\"",
            "\"net\"",
            "\"net/http\"",
            "\"unsafe\"",
            "\"reflect\"",
            "\"runtime\"",
            "\"syscall\"",
            "\"plugin\"",
            "\"database/sql\"",
            "\"crypto/tls\"",
            "\"crypto/x509\"",
            "\"io/ioutil\"",
            "\"path/filepath\"",
            "\"time\"",
            "\"context\"",
            "\"log\"",
            "\"testing\""
        };

        foreach (var import in dangerousImports)
        {
            if (code.Contains($"import {import}") ||
                code.Contains($"import (\n\t{import}") ||
                code.Contains($"import (\r\n\t{import}") ||
                code.Contains($"import(\n\t{import}") ||
                code.Contains($"import(\r\n\t{import}"))
            {
                throw new SecurityException($"Go code contains potentially unsafe import: {import}");
            }
        }
    }
}