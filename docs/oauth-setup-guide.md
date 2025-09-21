# OAuth Setup Guide for AscendDev

## Problem Summary

The OAuth authentication was failing with `redirect_uri_mismatch` errors because:

1. **Backend Configuration**: Was pointing to API endpoints (`https://localhost:7000/api/auth/oauth/{provider}/callback`)
2. **Frontend Implementation**: Was using frontend routes (`http://localhost:3000/auth/callback/{provider}`)
3. **OAuth Provider Registration**: Needed to match the actual redirect URIs being used

## Solution Implemented

### 1. Fixed Backend Configuration

Updated [`AscendDev.API/appsettings.json`](../AscendDev.API/appsettings.json) to use frontend callback URLs:

```json
{
  "OAuth": {
    "GitHub": {
      "ClientId": "your-github-client-id",
      "ClientSecret": "your-github-client-secret",
      "RedirectUri": "http://localhost:3000/auth/callback/github",
      "Scope": "user:email"
    },
    "Google": {
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret",
      "RedirectUri": "http://localhost:3000/auth/callback/google",
      "Scope": "openid profile email"
    }
  }
}
```

### 2. Enhanced Error Handling

Added comprehensive logging and error handling to [`GitHubOAuthProvider.cs`](../AscendDev.Services/Services/OAuth/GitHubOAuthProvider.cs) and [`OAuthService.cs`](../AscendDev.Services/Services/OAuthService.cs) for better debugging.

## Required OAuth Provider Configuration

### GitHub OAuth App Setup

1. Go to [GitHub Developer Settings](https://github.com/settings/developers)
2. Click "New OAuth App"
3. Fill in the details:
   - **Application name**: AscendDev
   - **Homepage URL**: `http://localhost:3000`
   - **Authorization callback URL**: `http://localhost:3000/auth/callback/github`
4. Copy the **Client ID** and **Client Secret**
5. Update `appsettings.json` with your credentials

### Google OAuth 2.0 Setup

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing one
3. Enable the Google+ API
4. Go to "Credentials" → "Create Credentials" → "OAuth 2.0 Client IDs"
5. Configure the OAuth consent screen first if prompted
6. For Application type, select "Web application"
7. Add authorized redirect URIs:
   - `http://localhost:3000/auth/callback/google`
8. Copy the **Client ID** and **Client Secret**
9. Update `appsettings.json` with your credentials

## OAuth Flow Architecture

```
1. User clicks OAuth button in frontend
   ↓
2. Frontend calls /api/auth/oauth/{provider}/authorize
   ↓
3. Backend generates authorization URL with state
   ↓
4. Frontend redirects user to OAuth provider
   ↓
5. User authorizes, provider redirects to: http://localhost:3000/auth/callback/{provider}?code=...&state=...
   ↓
6. Frontend OAuthCallbackPage handles the callback
   ↓
7. Frontend calls /api/auth/oauth/{provider}/callback with code and redirectUri
   ↓
8. Backend exchanges code for access token using the same redirectUri
   ↓
9. Backend fetches user info and creates/updates user account
   ↓
10. Backend returns JWT tokens to frontend
```

## Key Files Modified

1. **[`AscendDev.API/appsettings.json`](../AscendDev.API/appsettings.json)**: Updated redirect URIs
2. **[`AscendDev.Services/Services/OAuth/GitHubOAuthProvider.cs`](../AscendDev.Services/Services/OAuth/GitHubOAuthProvider.cs)**: Enhanced error handling and logging
3. **[`AscendDev.Services/Services/OAuthService.cs`](../AscendDev.Services/Services/OAuthService.cs)**: Added debug logging

## Frontend OAuth Flow

The frontend OAuth flow is handled by:

- **[`OAuthButton.tsx`](../frontend/src/components/auth/OAuthButton.tsx)**: Initiates OAuth flow
- **[`OAuthCallbackPage.tsx`](../frontend/src/pages/auth/OAuthCallbackPage.tsx)**: Handles OAuth callback
- **[`useOAuth.ts`](../frontend/src/hooks/api/useOAuth.ts)**: API hooks for OAuth operations

## Testing the OAuth Flow

1. **Start the backend API**:
   ```bash
   dotnet run --project AscendDev.API
   ```

2. **Start the frontend**:
   ```bash
   cd frontend
   npm run dev
   ```

3. **Configure OAuth providers** with the redirect URIs mentioned above

4. **Update credentials** in `appsettings.json`

5. **Test the flow**:
   - Navigate to `http://localhost:3000/login`
   - Click "Continue with GitHub" or "Continue with Google"
   - Complete OAuth authorization
   - Should redirect back and log you in

## Production Configuration

For production, update the redirect URIs to your production domain:

```json
{
  "OAuth": {
    "GitHub": {
      "RedirectUri": "https://yourdomain.com/auth/callback/github"
    },
    "Google": {
      "RedirectUri": "https://yourdomain.com/auth/callback/google"
    }
  }
}
```

And register the same URIs in your OAuth provider consoles.

## Troubleshooting

### Common Issues

1. **`redirect_uri_mismatch`**: Ensure the redirect URI in your OAuth provider console exactly matches the one in `appsettings.json`

2. **`bad_verification_code`** / **`invalid_grant`**: This usually means:
   - The authorization code has expired (codes are single-use and short-lived, typically 10 minutes)
   - The redirect URI used during token exchange doesn't match the one used during authorization
   - The code has already been used (OAuth codes are single-use only)
   - Clock skew between your server and the OAuth provider
   - **Solution**: Don't refresh the callback page or navigate back - this reuses the same code

3. **Google OAuth `invalid_grant` specifically**:
   - Ensure your Google OAuth app is configured for "Web application" type
   - Check that the authorized redirect URI exactly matches: `http://localhost:3000/auth/callback/google`
   - Make sure you're not testing with an expired or reused authorization code
   - Try clearing browser cache and cookies for Google accounts

4. **CORS issues**: Ensure your frontend domain is allowed in CORS configuration

5. **Multiple callback attempts**: If you see repeated failures with the same code, the user likely refreshed the callback page or navigated back, causing the same authorization code to be used multiple times

### Debug Logging

The enhanced error handling will now log:
- Redirect URIs being used
- HTTP response status codes
- Full error responses from OAuth providers
- Token exchange details (without exposing sensitive data)

Check the application logs for detailed error information.