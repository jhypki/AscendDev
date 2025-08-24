namespace AscendDev.Core.Models.Admin;

public class BulkOperation
{
    public Guid Id { get; set; }
    public string OperationType { get; set; } = null!;
    public Guid PerformedBy { get; set; }
    public int TargetCount { get; set; }
    public Dictionary<string, object>? OperationData { get; set; }
    public string Status { get; set; } = "pending"; // pending, in_progress, completed, failed
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}