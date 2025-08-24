namespace AscendDev.Core.Models.Admin;

public class UserActivityLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ActivityType { get; set; } = null!;
    public string? ActivityDescription { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? SessionId { get; set; }
    public DateTime CreatedAt { get; set; }
}