namespace AscendDev.Core.DTOs.Admin;

public class GenerateReportRequest
{
    public string ReportType { get; set; } = null!;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
    public string Format { get; set; } = "json"; // json, csv, pdf
}

public class ReportGenerationResponse
{
    public Guid ReportId { get; set; }
    public string ReportType { get; set; } = null!;
    public string Status { get; set; } = null!; // generating, completed, failed
    public string? DownloadUrl { get; set; }
    public DateTime GeneratedAt { get; set; }
    public object? Data { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ReportTypeResponse
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public List<ReportParameterInfo>? Parameters { get; set; }
}

public class ReportParameterInfo
{
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!; // string, date, number, boolean
    public bool Required { get; set; }
    public string? Description { get; set; }
    public object? DefaultValue { get; set; }
}

public class SystemSettingsResponse
{
    public bool MaintenanceMode { get; set; }
    public bool RegistrationEnabled { get; set; }
    public bool EmailNotificationsEnabled { get; set; }
    public int MaxUsersPerCourse { get; set; }
    public int SessionTimeoutMinutes { get; set; }
    public int BackupFrequencyHours { get; set; }
    public string? MaintenanceMessage { get; set; }
    public DateTime? LastBackup { get; set; }
}

public class UpdateSystemSettingsRequest
{
    public bool? MaintenanceMode { get; set; }
    public bool? RegistrationEnabled { get; set; }
    public bool? EmailNotificationsEnabled { get; set; }
    public int? MaxUsersPerCourse { get; set; }
    public int? SessionTimeoutMinutes { get; set; }
    public int? BackupFrequencyHours { get; set; }
    public string? MaintenanceMessage { get; set; }
}