
# OAuth Implementation Guide - GitHub and Google Integration

## Overview

This guide provides step-by-step instructions for implementing OAuth authentication with GitHub and Google providers in the AscendDev e-learning platform. OAuth integration will allow users to sign up and log in using their existing GitHub or Google accounts, improving user experience and reducing registration friction.

## Benefits of OAuth Integration

### User Experience Benefits
- **Simplified Registration**: No need to create new passwords
- **Faster Login Process**: One-click authentication
- **Trusted Providers**: Users trust GitHub and Google
- **Profile Information**: Automatic profile data population
- **Developer-Friendly**: GitHub integration appeals to programming learners

### Technical Benefits
- **Reduced Security Risk**: No password storage for OAuth users
- **Profile Enrichment**: Access to user's GitHub repositories and activity
- **Social Features**: Connect with GitHub followers/following
- **Professional Networking**: Link to professional profiles

## Architecture Overview

### OAuth Flow Integration
```
┌─────────────┐    ┌──────────────┐    ┌─────────────────┐    ┌──────────────┐
│   Frontend  │    │  AscendDev   │    │  OAuth Provider │    │  AscendDev   │
│   (React)   │    │     API      │    │ (GitHub/Google) │    │   Database   │
└─────────────┘    └──────────────┘    └─────────────────┘    └──────────────┘
       │                   │                      │                     │
       │ 1. Initiate OAuth │                      │                     │
       ├──────────────────►│                      │                     │
       │                   │ 2. Redirect to OAuth │                     │
       │                   ├─────────────────────►│                     │
       │ 3. User Authorizes│                      │                     │
       │◄──────────────────┼──────────────────────┤                     │
       │                   │ 4. Authorization Code│                     │
       │ 5. Send Auth Code │◄─────────────────────┤                     │
       ├──────────────────►│                      │                     │
       │                   │ 6. Exchange for Token│                     │
       │                   ├─────────────────────►│                     │
       │                   │ 7. Access Token      │                     │
       │                   │◄─────────────────────┤                     │
       │                   │ 8. Get User Profile  │                     │
       │                   ├─────────────────────►│                     │
       │                   │ 9. User Profile Data │                     │
       │                   │◄─────────────────────┤                     │
       │                   │ 10. Create/Update User                     │
       │                   ├────────────────────────────────────────────►│
       │ 11. JWT Token     │                      │                     │
       │◄──────────────────┤                      │                     │
```

## Implementation Steps

### 1. OAuth Provider Setup

#### 1.1 GitHub OAuth App Configuration

**Step 1: Create GitHub OAuth App**
1. Go to GitHub Settings → Developer settings → OAuth Apps
2. Click "New OAuth App"
3. Fill in application details:
   ```
   Application name: AscendDev Learning Platform
   Homepage URL: https://ascenddev.com
   Authorization callback URL: https://api.ascenddev.com/api/auth/github/callback
   ```
4. Note down `Client ID` and `Client Secret`

**Step 2: Configure Scopes**
Required scopes for AscendDev:
- `user:email` - Access user's email addresses
- `read:user` - Access user profile information
- `public_repo` - Access public repositories (optional, for portfolio features)

#### 1.2 Google OAuth App Configuration

**Step 1: Create Google OAuth App**
1. Go to Google Cloud Console
2. Create new project or select existing project
3. Enable Google+ API and Google OAuth2 API
4. Go to Credentials → Create Credentials → OAuth 2.0 Client IDs
5. Configure OAuth consent screen
6. Set up OAuth 2.0 Client ID:
   ```
   Application type: Web application
   Authorized JavaScript origins: https://ascenddev.com
   Authorized redirect URIs: https://api.ascenddev.com/api/auth/google/callback
   ```
7. Note down `Client ID` and `Client Secret`

**Step 2: Configure Scopes**
Required scopes for AscendDev:
- `openid` - OpenID Connect
- `email` - Access user's email address
- `profile` - Access user's basic profile information

### 2. Backend Implementation

#### 2.1 NuGet Package Installation
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.Google" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.OAuth" Version="8.0.0" />
<PackageReference Include="System.Text.Json" Version="8.0.0" />
```

#### 2.2 Configuration Setup

**appsettings.json**
```json
{
  "Authentication": {
    "Google": {
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret"
    },
    "GitHub": {
      "ClientId": "your-github-client-id",
      "ClientSecret": "your-github-client-secret"
    }
  }
}
```

**OAuth Configuration Models**
```csharp
public class OAuthSettings
{
    public GoogleOAuthSettings Google { get; set; } = new();
    public GitHubOAuthSettings GitHub { get; set; } = new();
}

public class GoogleOAuthSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public class GitHubOAuthSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}
```

#### 2.3 Authentication Service Configuration

**Program.cs**
```csharp
public static void Main(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);
    
    // Configure OAuth authentication
    builder.Services.AddOAuthAuthentication(builder.Configuration);
    
    var app = builder.Build();
    
    // Configure OAuth endpoints
    app.UseOAuthEndpoints();
    
    app.Run();
}
```

**OAuth Extensions**
```csharp
public static class OAuthExtensions
{
    public static IServiceCollection AddOAuthAuthentication(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var oauthSettings = configuration.GetSection("Authentication").Get<OAuthSettings>();
        
        services.Configure<OAuthSettings>(configuration.GetSection("Authentication"));
        
        services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.ClientId = oauthSettings.Google.ClientId;
                options.ClientSecret = oauthSettings.Google.ClientSecret;
                options.CallbackPath = "/api/auth/google/callback";
                options.Scope.Add("email");
                options.Scope.Add("profile");
                
                options.Events.OnCreatingTicket = async context =>
                {
                    // Extract additional user information
                    await ExtractGoogleUserInfo(context);
                };
            })
            .AddOAuth("GitHub", options =>
            {
                options.ClientId = oauthSettings.GitHub.ClientId;
                options.ClientSecret = oauthSettings.GitHub.ClientSecret;
                options.CallbackPath = "/api/auth/github/callback";
                
                options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                options.TokenEndpoint = "https://github.com/login/oauth/access_token";
                options.UserInformationEndpoint = "https://api.github.com/user";
                
                options.Scope.Add("user:email");
                options.Scope.Add("read:user");
                
                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                options.ClaimActions.MapJsonKey("urn:github:login", "login");
                options.ClaimActions.MapJsonKey("urn:github:url", "html_url");
                options.ClaimActions.MapJsonKey("urn:github:avatar", "avatar_url");
                
                options.Events.OnCreatingTicket = async context =>
                {
                    await ExtractGitHubUserInfo(context);
                };
            });
        
        services.AddScoped<IOAuthService, OAuthService>();
        
        return services;
    }
    
    private static async Task ExtractGoogleUserInfo(OAuthCreatingTicketContext context)
    {
        // Google automatically provides user info in the token response
        // Additional processing can be done here if needed
    }
    
    private static async Task ExtractGitHubUserInfo(OAuthCreatingTicketContext context)
    {
        // Get user's email addresses (GitHub may not provide email in user endpoint)
        var emailRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user/emails");
        emailRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
        emailRequest.Headers.UserAgent.TryParseAdd("AscendDev-OAuth");
        
        var emailResponse = await context.Backchannel.SendAsync(emailRequest);
        if (emailResponse.IsSuccessStatusCode)
        {
            var emailsJson = await emailResponse.Content.ReadAsStringAsync();
            var emails = JsonSerializer.Deserialize<GitHubEmail[]>(emailsJson);
            var primaryEmail = emails?.FirstOrDefault(e => e.Primary && e.Verified)?.Email;
            
            if (!string.IsNullOrEmpty(primaryEmail))
            {
                context.Identity?.AddClaim(new Claim(ClaimTypes.Email, primaryEmail));
            }
        }
    }
}

public class GitHubEmail
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("primary")]
    public bool Primary { get; set; }
    
    [JsonPropertyName("verified")]
    public bool Verified { get; set; }
}
```

#### 2.4 OAuth Controller Implementation

```csharp
[ApiController]
[Route("api/auth")]
public class OAuthController : ControllerBase
{
    private readonly IOAuthService _oauthService;
    private readonly ILogger<OAuthController> _logger;

    public OAuthController(IOAuthService oauthService, ILogger<OAuthController> logger)
    {
        _oauthService = oauthService;
        _logger = logger;
    }

    [HttpGet("google")]
    public IActionResult GoogleLogin([FromQuery] string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(GoogleCallback), "OAuth", new { returnUrl });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string? returnUrl = null)
    {
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        
        if (!result.Succeeded)
        {
            _logger.LogWarning("Google OAuth authentication failed");
            return BadRequest("Google authentication failed");
        }

        var oauthResult = await _oauthService.ProcessOAuthLogin(result.Principal, "Google");
        
        if (oauthResult.Success)
        {
            return Redirect($"{GetClientUrl()}/auth/success?token={oauthResult.Token}");
        }
        
        return Redirect($"{GetClientUrl()}/auth/error?message={oauthResult.ErrorMessage}");
    }

    [HttpGet("github")]
    public IActionResult GitHubLogin([FromQuery] string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(GitHubCallback), "OAuth", new { returnUrl });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, "GitHub");
    }

    [HttpGet("github/callback")]
    public async Task<IActionResult> GitHubCallback([FromQuery] string? returnUrl = null)
    {
        var result = await HttpContext.AuthenticateAsync("GitHub");
        
        if (!result.Succeeded)
        {
            _logger.LogWarning("GitHub OAuth authentication failed");
            return BadRequest("GitHub authentication failed");
        }

        var oauthResult = await _oauthService.ProcessOAuthLogin(result.Principal, "GitHub");
        
        if (oauthResult.Success)
        {
            return Redirect($"{GetClientUrl()}/auth/success?token={oauthResult.Token}");
        }
        
        return Redirect($"{GetClientUrl()}/auth/error?message={oauthResult.ErrorMessage}");
    }

    private string GetClientUrl()
    {
        // Return the frontend URL based on environment
        return Configuration.GetValue<string>("ClientUrl") ?? "http://localhost:3000";
    }
}
```

#### 2.5 OAuth Service Implementation

```csharp
public interface IOAuthService
{
    Task<OAuthResult> ProcessOAuthLogin(ClaimsPrincipal principal, string provider);
    Task<User?> FindOrCreateOAuthUser(OAuthUserInfo userInfo, string provider);
}

public class OAuthService : IOAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtHelper _jwtHelper;
    private readonly ILogger<OAuthService> _logger;

    public OAuthService(
        IUserRepository userRepository,
        IJwtHelper jwtHelper,
        ILogger<OAuthService> logger)
    {
        _userRepository = userRepository;
        _jwtHelper = jwtHelper;
        _logger = logger;
    }

    public async Task<OAuthResult> ProcessOAuthLogin(ClaimsPrincipal principal, string provider)
    {
        try
        {
            var userInfo = ExtractUserInfo(principal, provider);
            if (userInfo == null)
            {
                return OAuthResult.Failure("Failed to extract user information");
            }

            var user = await FindOrCreateOAuthUser(userInfo, provider);
            if (user == null)
            {
                return OAuthResult.Failure("Failed to create or find user");
            }

            // Update last login
            user.LastLogin = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            // Generate JWT token
            var token = _jwtHelper.GenerateToken(user);
            
            _logger.LogInformation("OAuth login successful for user {UserId} via {Provider}", 
                user.Id, provider);

            return OAuthResult.Success(token, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OAuth login for provider {Provider}", provider);
            return OAuthResult.Failure("Internal server error");
        }
    }

    public async Task<User?> FindOrCreateOAuthUser(OAuthUserInfo userInfo, string provider)
    {
        // First, try to find existing user by external ID
        var existingUser = await _userRepository.GetByExternalIdAsync(userInfo.ExternalId, provider);
        if (existingUser != null)
        {
            // Update user information if needed
            await UpdateUserFromOAuth(existingUser, userInfo);
            return existingUser;
        }

        // Try to find user by email
        if (!string.IsNullOrEmpty(userInfo.Email))
        {
            existingUser = await _userRepository.GetByEmailAsync(userInfo.Email);
            if (existingUser != null)
            {
                // Link OAuth account to existing user
                existingUser.ExternalId = userInfo.ExternalId;
                existingUser.Provider = provider;
                await _userRepository.UpdateAsync(existingUser);
                return existingUser;
            }
        }

        // Create new user
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Email = userInfo.Email ?? $"{userInfo.ExternalId}@{provider.ToLower()}.oauth",
            Username = userInfo.Username ?? GenerateUsername(userInfo.Name, provider),
            FirstName = userInfo.FirstName,
            LastName = userInfo.LastName,
            ProfilePictureUrl = userInfo.AvatarUrl,
            ExternalId = userInfo.ExternalId,
            Provider = provider,
            IsEmailVerified = true, // OAuth providers verify emails
            CreatedAt = DateTime.UtcNow
        };

        var created = await _userRepository.CreateAsync(newUser);
        return created ? newUser : null;
    }

    private OAuthUserInfo? ExtractUserInfo(ClaimsPrincipal principal, string provider)
    {
        return provider.ToLower() switch
        {
            "google" => ExtractGoogleUserInfo(principal),
            "github" => ExtractGitHubUserInfo(principal),
            _ => null
        };
    }

    private OAuthUserInfo ExtractGoogleUserInfo(ClaimsPrincipal principal)
    {
        return new OAuthUserInfo
        {
            ExternalId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "",
            Email = principal.FindFirst(ClaimTypes.Email)?.Value,
            Name = principal.FindFirst(ClaimTypes.Name)?.Value,
            FirstName = principal.FindFirst(ClaimTypes.GivenName)?.Value,
            LastName = principal.FindFirst(ClaimTypes.Surname)?.Value,
            AvatarUrl = principal.FindFirst("picture")?.Value
        };
    }

    private OAuthUserInfo ExtractGitHubUserInfo(ClaimsPrincipal principal)
    {
        var name = principal.FindFirst(ClaimTypes.Name)?.Value;
        var nameParts = name?.Split(' ', 2);
        
        return new OAuthUserInfo
        {
            ExternalId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "",
            Email = principal.FindFirst(ClaimTypes.Email)?.Value,
            Name = name,
            FirstName = nameParts?.FirstOrDefault(),
            LastName = nameParts?.Skip(1).FirstOrDefault(),
            Username = principal.FindFirst("urn:github:login")?.Value,
            AvatarUrl = principal.FindFirst("urn:github:avatar")?.Value,
            ProfileUrl = principal.FindFirst("urn:github:url")?.Value
        };
    }

    private async Task UpdateUserFromOAuth(User user, OAuthUserInfo userInfo)
    {
        var updated = false;

        if (string.IsNullOrEmpty(user.ProfilePictureUrl) && !string.IsNullOrEmpty(userInfo.AvatarUrl))
        {
            user.ProfilePictureUrl = userInfo.AvatarUrl;
            updated = true;
        }

        if (string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(userInfo.FirstName))
        {
            user.FirstName = userInfo.FirstName;
            updated = true;
        }

        if (string.IsNullOrEmpty(user.LastName) && !string.IsNullOrEmpty(userInfo.LastName))
        {
            user.LastName = userInfo.LastName;
            updated = true;
        }

        if (updated)
        {
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
        }
    }

    private string GenerateUsername(string? name, string provider)
    {
        var baseName = name?.Replace(" ", "").ToLower() ?? "user";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return $"{baseName}_{provider.ToLower()}_{timestamp}";
    }
}

public class OAuthUserInfo
{
    public string ExternalId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Username { get; set; }
    public string? AvatarUrl { get; set; }
    public string? ProfileUrl { get; set; }
}

public class OAuthResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public User? User { get; set; }
    public string? ErrorMessage { get; set; }

    public static OAuthResult Success(string token, User user) => new()
    {
        Success = true,
        Token = token,
        User = user
    };

    public static OAuthResult Failure(string errorMessage) => new()
    {
        Success = false,
        ErrorMessage = errorMessage
    };
}
```

### 3. Database Schema Updates

#### 3.1 User Table Modifications
```sql
-- Add OAuth-related columns to users table
ALTER TABLE users 
ADD COLUMN external_id VARCHAR(255),
ADD COLUMN provider VARCHAR(50),
ADD COLUMN profile_url VARCHAR(500);

-- Create index for OAuth lookups
CREATE INDEX idx_users_external_id_provider ON users(external_id, provider);

-- Make password_hash nullable for OAuth users
ALTER TABLE users ALTER COLUMN password_hash DROP NOT NULL;
```

#### 3.2 OAuth Account Linking Table (Optional)
```sql
-- Optional: Separate table for multiple OAuth accounts per user
CREATE TABLE user_oauth_accounts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    provider VARCHAR(50) NOT NULL,
    external_id VARCHAR(255) NOT NULL,
    email VARCHAR(255),
    profile_url VARCHAR(500),
    access_token_hash VARCHAR(255), -- Store hashed token if needed
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    UNIQUE(provider, external_id)
);

CREATE INDEX idx_oauth_accounts_user_id ON user_oauth_accounts(user_id);
```

### 4. Frontend Integration

#### 4.1 OAuth Login Components

**React OAuth Login Component**
```typescript
import React from 'react';

interface OAuthLoginProps {
  provider: 'google' | 'github';
  onSuccess?: (token: string) => void;
  onError?: (error: string) => void;
}

export const OAuthLogin: React.FC<OAuthLoginProps> = ({ 
  provider, 
  onSuccess, 
  onError 
}) => {
  const handleOAuthLogin = () => {
    const apiUrl = process.env.REACT_APP_API_URL || 'http://localhost:5000';
    const oauthUrl = `${apiUrl}/api/auth/${provider}`;
    
    // Open OAuth popup
    const popup = window.open(
      oauthUrl,
      `${provider}-oauth`,
      'width=500,height=600,scrollbars=yes,resizable=yes'
    );

    // Listen for OAuth completion
    const checkClosed = setInterval(() => {
      if (popup?.closed) {
        clearInterval(checkClosed);
        // Check for success/error in localStorage or URL params
        const token = localStorage.getItem('oauth_token');
        const error = localStorage.getItem('oauth_error');
        
        if (token) {
          localStorage.removeItem('oauth_token');
          onSuccess?.(token);
        } else if (error) {
          localStorage.removeItem('oauth_error');
          onError?.(error);
        }
      }
    }, 1000);
  };

  const getProviderInfo = () => {
    switch (provider) {
      case 'google':
        return {
          name: 'Google',
          icon: '🔍',
          color: '#4285f4'
        };
      case 'github':
        return {
          name: 'GitHub',
          icon: '🐙',
          color: '#333'
        };
      default:
        return { name: provider, icon: '🔐', color: '#666' };
    }
  };

  const providerInfo = getProviderInfo();

  return (
    <button
      onClick={handleOAuthLogin}
      className={`oauth-button oauth-${provider}`}
      style={{ backgroundColor: providerInfo.color }}
    >
      <span className="oauth-icon">{providerInfo.icon}</span>
      Continue with {providerInfo.name}
    </button>
  );
};
```

**OAuth Success/Error Pages**
```typescript
// pages/auth/success.tsx
import { useEffect } from 'react';
import { useRouter } from 'next/router';

export default function OAuthSuccess() {
  const router = useRouter();
  
  useEffect(() => {
    const { token } = router.query;
    
    if (token) {
      // Store token and redirect to dashboard
      localStorage.setItem('oauth_token', token as string);
      window.close(); // Close popup
      // Or redirect to dashboard if not in popup
      router.push('/dashboard');
    }
  }, [router.query]);

  return (
    <div className="oauth-success">
      <h2>Authentication Successful!</h2>
      <p>You can now close this window.</p>
    </div>
  );
}

// pages/auth/error.tsx
import { useEffect } from 'react';
import { useRouter } from 'next/router';

export default function OAuthError() {
  const router = useRouter();
  
  useEffect(() => {
    const { message } = router.query;
    
    if (message) {
      localStorage.setItem('oauth_error', message as string);
      setTimeout(() => window.close(), 3000);
    }
  }, [router.query]);

  return (
    <div className="oauth-error">
      <h2>Authentication Failed</h2>
      <p>{router.query.message || 'An error occurred during authentication.'}</p>
      <p>This window will close automatically.</p>
    </div>
  );
}
```

### 5. Security Considerations

#### 5.1 Security Best Practices

**State Parameter Validation**
```csharp
public class OAuthStateService
{
    private readonly IMemoryCache _cache;
    
    public string GenerateState()
    {
        var state = Guid.NewGuid().ToString("N");
        _cache.Set($"oauth_state_{state}", true, TimeSpan.FromMinutes(10));
        return state;
    }
    
    public bool ValidateState(string state)
    {
        var key = $"oauth_state_{state}";
        if (_cache.TryGetValue(key, out _))
        {
            _cache.Remove(key);
            return true;
        }
        return false;
    }
}
```

**PKCE Implementation (for enhanced security)**
```csharp
public class PKCEService
{
    public (string codeVerifier, string codeChallenge) GeneratePKCE()
    {
        var codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);
        return (codeVerifier, codeChallenge);
    }
    
    private string GenerateCodeVerifier()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
    
    private string GenerateCodeChallenge(string codeVerifier)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier));
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
```

#### 5.2 Rate Limiting and Abuse Prevention

```csharp
[EnableRateLimiting("OAuthPolicy")]
public class OAuthController : ControllerBase
{
    // OAuth endpoints with rate limiting
}

// In Program.cs
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("OAuthPolicy", configure =>
    {
        configure.PermitLimit = 10;
        configure.Window = TimeSpan.FromMinutes(1);
        configure.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        configure.QueueLimit = 5;
    });
});
```

### 6. Testing Strategy

#### 6.1 Unit Tests

```csharp
[TestClass]
public class OAuthServiceTests
{
    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<IJwtHelper> _jwtHelperMock;
    private OAuthService _oauthService;

    [TestInitialize]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _jwtHelperMock = new Mock<IJwtHelper>();
        _oauthService = new OAuthService(
            _userRepositoryMock.Object,
            _jwtHelperMock.Object,
            Mock.Of<ILogger<OAuthService>>());
    }

    [TestMethod]
    public async Task ProcessOAuthLogin_NewUser_CreatesUserAndReturnsToken()
    {
        // Arrange
        var principal = CreateTestPrincipal("google");
        _userRepositoryMock.Setup(x => x.GetByExternalIdAsync(It.IsAny<string>(), "Google"))
            .ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(true);
        _jwtHelperMock.Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("test-token");

        // Act
        var result = await _oauthService.ProcessOAuthLogin(principal, "Google");

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("test-token", result.Token);
        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
    }

    private ClaimsPrincipal CreateTestPrincipal(string provider)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "12345"),
            new(ClaimTypes.Email, "test@example.com"),
            new(ClaimTypes.Name, "Test User")
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, provider));
    }
}
```

#### 6.2 Integration Tests

```csharp
[TestClass]
public class OAuthIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public OAuthIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [TestMethod]
    public async Task GoogleOAuth_RedirectsToGoogleAuthorizationEndpoint()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/google");

        // Assert
        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.IsTrue(response.Headers.Location?.ToString().Contains("accounts.google.com"));
    }
}
```

### 7. Monitoring and Analytics

#### 7.1 OAuth Metrics Collection

```csharp
public class OAuthMetricsService
{
    private readonly IMetricsCollector _metrics;

    public void RecordOAuthAttem