namespace AscendDev.Core.Models.TestsExecution;

public class PerformanceMetrics
{
    /// <summary>
    /// Total execution time in milliseconds (includes all overhead)
    /// </summary>
    public long TotalExecutionTimeMs { get; set; }

    /// <summary>
    /// Pure test execution time in milliseconds (from test framework, excluding container overhead)
    /// </summary>
    public double? PureTestExecutionTimeMs { get; set; }

    /// <summary>
    /// Container startup time in milliseconds
    /// </summary>
    public long? ContainerStartupTimeMs { get; set; }

    /// <summary>
    /// Container cleanup time in milliseconds
    /// </summary>
    public long? ContainerCleanupTimeMs { get; set; }

    /// <summary>
    /// File preparation time in milliseconds
    /// </summary>
    public long? FilePreparationTimeMs { get; set; }

    /// <summary>
    /// Test framework execution time in milliseconds (container-level timing)
    /// </summary>
    public long? ContainerExecutionTimeMs { get; set; }

    /// <summary>
    /// Infrastructure overhead time in milliseconds (total - pure execution)
    /// </summary>
    public long? InfrastructureOverheadMs => TotalExecutionTimeMs - (long)(PureTestExecutionTimeMs ?? 0);

    /// <summary>
    /// Memory usage in MB (if available)
    /// </summary>
    public double? MemoryUsageMb { get; set; }

    /// <summary>
    /// CPU usage percentage (if available)
    /// </summary>
    public double? CpuUsagePercent { get; set; }

    /// <summary>
    /// Number of tests executed
    /// </summary>
    public int TestCount { get; set; }

    /// <summary>
    /// Average time per test in milliseconds (based on pure execution time)
    /// </summary>
    public double? AverageTestTimeMs => TestCount > 0 && PureTestExecutionTimeMs.HasValue
        ? PureTestExecutionTimeMs.Value / TestCount
        : null;

    /// <summary>
    /// Peak memory usage during execution in MB
    /// </summary>
    public double? PeakMemoryUsageMb { get; set; }

    /// <summary>
    /// Additional performance data as key-value pairs
    /// </summary>
    public Dictionary<string, object> AdditionalMetrics { get; set; } = new();

    /// <summary>
    /// Legacy property for backward compatibility
    /// </summary>
    [Obsolete("Use TotalExecutionTimeMs instead")]
    public long ExecutionTimeMs
    {
        get => TotalExecutionTimeMs;
        set => TotalExecutionTimeMs = value;
    }

    /// <summary>
    /// Legacy property for backward compatibility
    /// </summary>
    [Obsolete("Use ContainerExecutionTimeMs instead")]
    public long? TestFrameworkTimeMs
    {
        get => ContainerExecutionTimeMs;
        set => ContainerExecutionTimeMs = value;
    }
}