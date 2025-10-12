using System.Net;
using System.Net.Mail;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Models.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AscendDev.Services.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        _logger.LogInformation("Attempting to send email to {To} with subject: {Subject}", to, subject);
        _logger.LogDebug("SMTP Settings: Host={Host}, Port={Port}, EnableSsl={EnableSsl}, Username={Username}",
            _emailSettings.SmtpHost, _emailSettings.SmtpPort, _emailSettings.EnableSsl, _emailSettings.SmtpUsername);

        // Validate email settings
        if (string.IsNullOrEmpty(_emailSettings.SmtpHost) ||
            string.IsNullOrEmpty(_emailSettings.SmtpUsername) ||
            string.IsNullOrEmpty(_emailSettings.SmtpPassword))
        {
            _logger.LogError("Email settings are not properly configured. Please check SMTP configuration.");
            return;
        }

        try
        {
            using var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
            {
                EnableSsl = _emailSettings.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 60000 // 60 seconds timeout
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            mailMessage.To.Add(to);

            _logger.LogInformation("Connecting to SMTP server and sending email...");
            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {To} with subject: {Subject}", to, subject);
        }
        catch (SmtpException smtpEx)
        {
            _logger.LogError(smtpEx, "SMTP error occurred while sending email to {To}. StatusCode: {StatusCode}, Error: {ErrorMessage}",
                to, smtpEx.StatusCode, smtpEx.Message);

            // Log specific guidance for common SMTP errors
            if (smtpEx.Message.Contains("Authentication Required") || smtpEx.Message.Contains("5.7.0"))
            {
                _logger.LogError("SMTP Authentication failed. For Gmail, ensure you're using an App Password instead of your regular password. " +
                    "Enable 2FA and generate an App Password at: https://myaccount.google.com/apppasswords");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To} with subject: {Subject}. Error: {ErrorMessage}", to, subject, ex.Message);
        }
    }

    public async Task SendCodeReviewNotificationEmailAsync(string to, string reviewerName, string lessonTitle, string actionUrl)
    {
        var subject = "New Code Review - AscendDev";
        var body = GenerateCodeReviewEmailBody(reviewerName, lessonTitle, actionUrl);

        await SendEmailAsync(to, subject, body, true);
    }

    public async Task SendCodeReviewCommentNotificationEmailAsync(string to, string commenterName, string lessonTitle, string actionUrl)
    {
        var subject = "New Code Review Comment - AscendDev";
        var body = GenerateCodeReviewCommentEmailBody(commenterName, lessonTitle, actionUrl);

        await SendEmailAsync(to, subject, body, true);
    }

    public async Task SendEmailVerificationAsync(string to, string verificationUrl, string username)
    {
        var subject = "Verify Your Email Address - AscendDev";
        var body = GenerateEmailVerificationBody(verificationUrl, username);

        await SendEmailAsync(to, subject, body, true);
    }

    public async Task SendWelcomeEmailAsync(string to, string username)
    {
        var subject = "Welcome to AscendDev! 🎉";
        var body = GenerateWelcomeEmailBody(username);

        await SendEmailAsync(to, subject, body, true);
    }

    private string GenerateEmailVerificationBody(string verificationUrl, string username)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Verify Your Email Address</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f8f9fa; padding: 30px; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; background-color: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 14px; }}
        .warning {{ background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 4px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>📧 Verify Your Email Address</h1>
    </div>
    <div class='content'>
        <h2>Hello {username}!</h2>
        <p>Thank you for creating an account with AscendDev! To complete your registration and start your coding journey, please verify your email address.</p>
        <p>Click the button below to verify your email:</p>
        <a href='{verificationUrl}' class='button'>Verify Email Address</a>
        <div class='warning'>
            <p><strong>⚠️ Important:</strong> This verification link will expire in 24 hours for security reasons. If you don't verify your email within this time, you'll need to request a new verification email.</p>
        </div>
        <p>If the button doesn't work, you can copy and paste this link into your browser:</p>
        <p style='word-break: break-all; background-color: #e9ecef; padding: 10px; border-radius: 4px; font-family: monospace;'>{verificationUrl}</p>
        <p>If you didn't create an account with AscendDev, please ignore this email.</p>
    </div>
    <div class='footer'>
        <p>Best regards,<br>The AscendDev Team</p>
        <p><small>This is an automated email. Please do not reply to this message.</small></p>
    </div>
</body>
</html>";
    }

    private string GenerateWelcomeEmailBody(string username)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Welcome to AscendDev!</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f8f9fa; padding: 30px; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; background-color: #28a745; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 14px; }}
        .feature {{ background-color: white; padding: 15px; margin: 10px 0; border-radius: 4px; border-left: 4px solid #28a745; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>🎉 Welcome to AscendDev!</h1>
    </div>
    <div class='content'>
        <h2>Hello {username}!</h2>
        <p>Congratulations! Your email has been verified and your AscendDev account is now active. We're excited to have you join our community of learners and developers!</p>
        
        <h3>🚀 What's Next?</h3>
        <div class='feature'>
            <h4>📚 Explore Courses</h4>
            <p>Browse our comprehensive programming courses covering Python, JavaScript, TypeScript, Go, and more!</p>
        </div>
        <div class='feature'>
            <h4>💻 Start Coding</h4>
            <p>Jump into interactive coding exercises with our built-in code editor and automated testing.</p>
        </div>
        <div class='feature'>
            <h4>👥 Join the Community</h4>
            <p>Connect with other learners, participate in discussions, and get code reviews from peers.</p>
        </div>
        
        <p>Ready to start your coding journey?</p>
        <a href='https://ascenddev.com/dashboard' class='button'>Go to Dashboard</a>
        
        <p>If you have any questions or need help getting started, don't hesitate to reach out to our support team.</p>
    </div>
    <div class='footer'>
        <p>Happy coding!<br>The AscendDev Team</p>
        <p><small>You can manage your email preferences in your account settings.</small></p>
    </div>
</body>
</html>";
    }

    private string GenerateCodeReviewEmailBody(string reviewerName, string lessonTitle, string actionUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>New Code Review</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f8f9fa; padding: 30px; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; background-color: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 14px; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>🔍 New Code Review</h1>
    </div>
    <div class='content'>
        <h2>Hello!</h2>
        <p><strong>{reviewerName}</strong> has reviewed your submission for the lesson <strong>'{lessonTitle}'</strong>.</p>
        <p>Click the button below to view the code review and any feedback provided:</p>
        <a href='{actionUrl}' class='button'>View Code Review</a>
        <p>This review will help you improve your coding skills and understanding of the concepts.</p>
    </div>
    <div class='footer'>
        <p>Best regards,<br>The AscendDev Team</p>
        <p><small>You received this email because you have email notifications enabled for code reviews. You can change your notification preferences in your account settings.</small></p>
    </div>
</body>
</html>";
    }

    private string GenerateCodeReviewCommentEmailBody(string commenterName, string lessonTitle, string actionUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>New Code Review Comment</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f8f9fa; padding: 30px; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; background-color: #28a745; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 14px; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>💬 New Comment on Your Code Review</h1>
    </div>
    <div class='content'>
        <h2>Hello!</h2>
        <p><strong>{commenterName}</strong> has commented on your code review for the lesson <strong>'{lessonTitle}'</strong>.</p>
        <p>Click the button below to view the comment and continue the discussion:</p>
        <a href='{actionUrl}' class='button'>View Comment</a>
        <p>Engaging in code review discussions is a great way to learn and improve your programming skills.</p>
    </div>
    <div class='footer'>
        <p>Best regards,<br>The AscendDev Team</p>
        <p><small>You received this email because you have email notifications enabled for code reviews. You can change your notification preferences in your account settings.</small></p>
    </div>
</body>
</html>";
    }
}