namespace AscendDev.Core.Interfaces.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task SendCodeReviewNotificationEmailAsync(string to, string reviewerName, string lessonTitle, string actionUrl);
    Task SendCodeReviewCommentNotificationEmailAsync(string to, string commenterName, string lessonTitle, string actionUrl);
    Task SendEmailVerificationAsync(string to, string verificationUrl, string username);
    Task SendWelcomeEmailAsync(string to, string username);
}