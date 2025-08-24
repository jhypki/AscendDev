namespace AscendDev.Core.Models.Admin;

public class DashboardStatistic
{
    public Guid Id { get; set; }
    public string StatisticKey { get; set; } = null!;
    public Dictionary<string, object> StatisticValue { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public DateTime? ExpiresAt { get; set; }
}