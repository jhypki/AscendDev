namespace AscendDev.Core.Models.TestsExecution;

public class PerformanceMetrics
{
    /// <summary>
    /// Total execution time in milliseconds
    /// </summary>
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// Memory usage in MB (if available)
    /// </summary>
    public double? MemoryUsageMb { get; set; }

    /// <summary>
    /// CPU usage percentage (if available)
    /// </summary>
    public double? CpuUsagePercent { get; set; }

    /// <summary>
    /// Container startup time in milliseconds
    /// </summary>
    public long? ContainerStartupTimeMs { get; set; }

    /// <summary>
    /// Test framework execution time in milliseconds (excluding container overhead)
    /// </summary>
    public long? TestFrameworkTimeMs { get; set; }

    /// <summary>
    /// Number of tests executed
    /// </summary>
    public int TestCount { get; set; }

    /// <summary>
    /// Average time per test in milliseconds
    /// </summary>
    public double? AverageTestTimeMs { get; set; }

    /// <summary>
    /// Peak memory usage during execution in MB
    /// </summary>
    public double? PeakMemoryUsageMb { get; set; }

    /// <summary>
    /// Additional performance data as key-value pairs
    /// </summary>
    public Dictionary<string, object> AdditionalMetrics { get; set; } = new();
}