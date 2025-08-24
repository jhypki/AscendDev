namespace AscendDev.Core.Models.Configuration;

public class SecuritySettings
{
    public const string SectionName = "Security";

    public int MaxFailedLoginAttempts { get; set; } = 5;
    public int AccountLockoutDurationMinutes { get; set; } = 30;
    public int EmailVerificationTokenExpirationHours { get; set; } = 24;
    public int PasswordResetTokenExpirationHours { get; set; } = 2;
    public bool RequireEmailVerification { get; set; } = true;
}

public class EmailSettings
{
    public const string SectionName = "Email";

    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
}

public class FeatureSettings
{
    public const string SectionName = "Features";

    public bool EnableOAuth { get; set; } = true;
    public bool EnableEmailNotifications { get; set; } = true;
    public bool EnableAchievements { get; set; } = true;
    public bool EnableDiscussions { get; set; } = true;
    public bool EnableCodeReviews { get; set; } = true;
    public bool EnableRealTimeNotifications { get; set; } = true;
}

public class RateLimitSettings
{
    public const string SectionName = "RateLimiting";

    public RateLimitPolicy LoginAttempts { get; set; } = new();
    public RateLimitPolicy ApiCalls { get; set; } = new();
    public RateLimitPolicy EmailSending { get; set; } = new();
}

public class RateLimitPolicy
{
    public int PermitLimit { get; set; } = 100;
    public int WindowMinutes { get; set; } = 1;
}

public class CachingSettings
{
    public const string SectionName = "Caching";

    public int DefaultExpirationMinutes { get; set; } = 30;
    public int UserProfileExpirationMinutes { get; set; } = 60;
    public int CourseDataExpirationMinutes { get; set; } = 120;
    public int AchievementsExpirationMinutes { get; set; } = 240;
}