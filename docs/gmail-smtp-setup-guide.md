# Gmail SMTP Setup Guide for AscendDev

This guide explains how to configure Gmail SMTP for sending emails from the AscendDev platform.

## Problem

Gmail requires App Passwords for SMTP authentication when using third-party applications. Regular Gmail passwords will result in authentication errors like:

```
The SMTP server requires a secure connection or the client was not authenticated. 
The server response was: 5.7.0 Authentication Required.
```

## Solution: Generate Gmail App Password

### Step 1: Enable 2-Factor Authentication

1. Go to your [Google Account settings](https://myaccount.google.com/)
2. Navigate to **Security** → **2-Step Verification**
3. Follow the prompts to enable 2FA if not already enabled

### Step 2: Generate App Password

1. In your Google Account, go to **Security** → **2-Step Verification** → **App passwords**
2. Select **Mail** as the app type
3. Generate a new app password
4. Copy the 16-character app password (format: `xxxx xxxx xxxx xxxx`)

### Step 3: Update Configuration

Update your `appsettings.json` with the app password:

```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-16-character-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "AscendDev Platform",
    "EnableSsl": true
  }
}
```

**Important**: Use the 16-character app password, NOT your regular Gmail password.

## Gmail SMTP Settings

| Setting | Value |
|---------|-------|
| SMTP Server | smtp.gmail.com |
| Port | 587 (STARTTLS) or 465 (SSL) |
| Security | TLS/SSL |
| Authentication | Required |
| Username | Your full Gmail address |
| Password | App Password (16 characters) |

## Testing

After updating the configuration:

1. Restart your application
2. Trigger an email notification (e.g., code review comment)
3. Check the application logs for successful email delivery

## Troubleshooting

### Common Issues

1. **"Authentication Required" Error**
   - Ensure you're using an App Password, not your regular password
   - Verify 2FA is enabled on your Google account

2. **"Less Secure App Access" Error**
   - This setting is deprecated. Use App Passwords instead

3. **Connection Timeout**
   - Check firewall settings
   - Verify SMTP server and port settings

### Alternative: OAuth2 Authentication

For production environments, consider implementing OAuth2 authentication instead of App Passwords for enhanced security. See the [MailKit OAuth2 documentation](https://github.com/jstedfast/MailKit/blob/master/GMailOAuth2.md) for implementation details.

## Security Best Practices

1. **Never commit passwords to version control**
   - Use environment variables or secure configuration management
   - Consider using Azure Key Vault or similar services

2. **Rotate App Passwords regularly**
   - Generate new app passwords periodically
   - Revoke unused app passwords

3. **Monitor email sending**
   - Implement rate limiting
   - Log email sending activities
   - Set up alerts for authentication failures

## Environment Variables

For production deployment, use environment variables:

```bash
Email__SmtpHost=smtp.gmail.com
Email__SmtpPort=587
Email__SmtpUsername=your-email@gmail.com
Email__SmtpPassword=your-app-password
Email__FromEmail=your-email@gmail.com
Email__FromName="AscendDev Platform"
Email__EnableSsl=true
```

## References

- [Google App Passwords Documentation](https://support.google.com/accounts/answer/185833)
- [MailKit Gmail Documentation](https://github.com/jstedfast/MailKit/blob/master/FAQ.md)
- [Gmail SMTP Settings](https://support.google.com/mail/answer/7126229)