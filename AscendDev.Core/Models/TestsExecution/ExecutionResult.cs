namespace AscendDev.Core.Models.CodeExecution;

public class ExecutionResult
{
    public bool Success { get; set; }
    public string Output { get; set; } = string.Empty;
    public int ExecutionTimeMs { get; set; }
    public bool TimedOut { get; set; }
}