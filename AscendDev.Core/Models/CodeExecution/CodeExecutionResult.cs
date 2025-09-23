using AscendDev.Core.Models.TestsExecution;

namespace AscendDev.Core.Models.CodeExecution;

public class CodeExecutionResult
{
    public bool Success { get; set; }
    public string Stdout { get; set; } = string.Empty;
    public string Stderr { get; set; } = string.Empty;
    public int ExitCode { get; set; }
    public string? CompilationOutput { get; set; }

    /// <summary>
    /// Enhanced performance metrics including pure execution time
    /// </summary>
    public PerformanceMetrics? Performance { get; set; }

    /// <summary>
    /// Legacy property for backward compatibility
    /// </summary>
    [Obsolete("Use Performance.PureTestExecutionTimeMs or Performance.TotalExecutionTimeMs instead")]
    public long ExecutionTimeMs
    {
        get => Performance?.TotalExecutionTimeMs ?? 0;
        set
        {
            Performance ??= new PerformanceMetrics();
            Performance.TotalExecutionTimeMs = value;
        }
    }
}