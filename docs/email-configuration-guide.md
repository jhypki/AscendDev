# Email Configuration Guide for AscendDev

This guide explains how to configure email notifications in the AscendDev platform, including troubleshooting common SMTP authentication issues.

## Overview

The AscendDev platform includes an email notification system that sends notifications for:
- New code review comments
- Code review submissions
- Other platform activities (extensible)

## Current Configuration

The email service is configured in [`AscendDev.API/appsettings.json`](../AscendDev.API/appsettings.json) under the `Email` section:

```json
{
  "Email": {
    "SmtpHost": "smtp.live.com",
    "SmtpPort": 587,
    "SmtpUsername": "ascenddev0@hotmail.com",
    "SmtpPassword": "HdkYYmoY1q9oqQWa",
    "FromEmail": "ascenddev0@hotmail.com",
    "FromName": "AscendDev Platform",
    "EnableSsl": true
  }
}
```

## Development Mode

The email service automatically falls back to development mode when:
- SMTP authentication fails
- Password is not configured or set to placeholder values
- SMTP server is unreachable

In development mode, email content is logged to the console instead of being sent, allowing developers to see what emails would be sent without requiring a working SMTP configuration.

## Microsoft Outlook/Hotmail SMTP Authentication Issues

### The Problem

Microsoft has deprecated basic authentication (username/password) for SMTP access and now requires:
1. **App-specific passwords** for personal accounts
2. **OAuth2 authentication** for organizational accounts
3. **Modern authentication protocols**

### Error Messages

You may see errors like:
```
5.7.57 Client not authenticated to send mail. Error: 535 5.7.3 Authentication unsuccessful
```

### Solutions

#### Option 1: App-Specific Passwords (Recommended for Development)

1. **Enable 2FA** on your Microsoft account
2. **Generate an app-specific password**:
   - Go to [Microsoft Account Security](https://account.microsoft.com/security)
   - Select "Advanced security options"
   - Under "App passwords", select "Create a new app password"
   - Use this generated password instead of your regular password

3. **Update configuration**:
```json
{
  "Email": {
    "SmtpHost": "smtp-mail.outlook.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@hotmail.com",
    "SmtpPassword": "your-app-specific-password",
    "FromEmail": "your-email@hotmail.com",
    "FromName": "AscendDev Platform",
    "EnableSsl": true
  }
}
```

#### Option 2: Alternative Email Providers

For easier setup, consider using:

**Gmail with App Passwords:**
```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "AscendDev Platform",
    "EnableSsl": true
  }
}
```

**SendGrid (Recommended for Production):**
```json
{
  "Email": {
    "SmtpHost": "smtp.sendgrid.net",
    "SmtpPort": 587,
    "SmtpUsername": "apikey",
    "SmtpPassword": "your-sendgrid-api-key",
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "AscendDev Platform",
    "EnableSsl": true
  }
}
```

#### Option 3: OAuth2 Implementation (Production)

For production environments, implement OAuth2 authentication:

1. **Register application** with Microsoft Azure AD
2. **Configure OAuth2 scopes** for SMTP access
3. **Implement token refresh logic**
4. **Update EmailService** to use OAuth2 tokens

This requires significant development work and is recommended for production deployments.

## Testing Email Configuration

### Using the Test Endpoint

The platform includes a test endpoint for verifying email configuration:

```http
POST /api/notifications/test-email
Content-Type: application/json
Authorization: Bearer your-jwt-token

{
  "to": "test@example.com"
}
```

### Checking Logs

Monitor the application logs for email-related messages:
- **Success**: `Email sent successfully to {email}`
- **Development Mode**: `Email service is in development mode`
- **Failure**: `Failed to send email to {email}`

## Email Service Implementation

The email service ([`AscendDev.Services/Services/EmailService.cs`](../AscendDev.Services/Services/EmailService.cs)) includes:

### Features
- **Graceful fallback** to development mode
- **Comprehensive logging** for debugging
- **HTML email templates** for notifications
- **Background processing** to prevent blocking
- **Error handling** that doesn't break core functionality

### Email Templates

The service includes pre-built templates for:
- **Code review notifications**
- **Code review comment notifications**
- **Extensible template system** for future notification types

## User Notification Preferences

Users can control email notifications through their settings:
- **Email notifications enabled/disabled**
- **Notification types** (code reviews, comments, etc.)
- **Frequency settings** (immediate, daily digest, etc.)

Settings are managed in the user preferences system and checked before sending emails.

## Production Deployment Recommendations

### Security
- **Never commit** SMTP passwords to version control
- **Use environment variables** for sensitive configuration
- **Implement OAuth2** for Microsoft accounts
- **Use dedicated email services** (SendGrid, AWS SES, etc.)

### Reliability
- **Configure retry logic** for failed email sends
- **Implement email queuing** for high-volume scenarios
- **Monitor email delivery rates** and bounce handling
- **Set up email reputation monitoring**

### Configuration Management
```json
{
  "Email": {
    "SmtpHost": "${SMTP_HOST}",
    "SmtpPort": "${SMTP_PORT}",
    "SmtpUsername": "${SMTP_USERNAME}",
    "SmtpPassword": "${SMTP_PASSWORD}",
    "FromEmail": "${FROM_EMAIL}",
    "FromName": "AscendDev Platform",
    "EnableSsl": true
  }
}
```

## Troubleshooting

### Common Issues

1. **Authentication Failures**
   - Verify app-specific passwords are enabled
   - Check 2FA is configured for the account
   - Ensure correct SMTP server and port

2. **Connection Timeouts**
   - Verify firewall settings allow SMTP traffic
   - Check network connectivity to SMTP server
   - Confirm SSL/TLS settings match server requirements

3. **Email Not Received**
   - Check spam/junk folders
   - Verify recipient email addresses
   - Monitor email service logs for delivery status

### Debug Mode

Enable detailed logging by setting log level to `Debug` in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "AscendDev.Services.Services.EmailService": "Debug"
    }
  }
}
```

## Future Enhancements

- **Email templates management UI**
- **Bulk email operations**
- **Email analytics and tracking**
- **Advanced user preference controls**
- **Integration with external email services**

## Support

For email configuration issues:
1. Check the application logs
2. Verify SMTP server settings
3. Test with the provided test endpoint
4. Review this documentation for common solutions

The email service is designed to be resilient - if email sending fails, the core application functionality (like creating code review comments) will continue to work normally.