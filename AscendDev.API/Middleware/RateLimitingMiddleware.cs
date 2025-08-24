using AscendDev.Core.Models.Configuration;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net;

namespace AscendDev.API.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimitSettings _settings;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, ClientRateLimit> _clients = new();

    public RateLimitingMiddleware(
        RequestDelegate next,
        IOptions<RateLimitSettings> settings,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        var endpoint = GetEndpointIdentifier(context);

        var rateLimitPolicy = GetRateLimitPolicy(endpoint);
        if (rateLimitPolicy == null)
        {
            await _next(context);
            return;
        }

        var key = $"{clientId}:{endpoint}";
        var clientLimit = _clients.GetOrAdd(key, _ => new ClientRateLimit());

        bool limitExceeded = false;
        int remainingRequests = 0;

        lock (clientLimit)
        {
            var now = DateTime.UtcNow;
            var windowStart = now.AddMinutes(-rateLimitPolicy.WindowMinutes);

            // Remove old requests outside the window
            clientLimit.Requests.RemoveAll(r => r < windowStart);

            // Check if limit exceeded
            if (clientLimit.Requests.Count >= rateLimitPolicy.PermitLimit)
            {
                limitExceeded = true;
            }
            else
            {
                // Add current request
                clientLimit.Requests.Add(now);
            }

            remainingRequests = Math.Max(0, rateLimitPolicy.PermitLimit - clientLimit.Requests.Count);
        }

        if (limitExceeded)
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientId} on endpoint {Endpoint}",
                clientId, endpoint);

            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers["Retry-After"] = (rateLimitPolicy.WindowMinutes * 60).ToString();

            await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
            return;
        }

        // Add rate limit headers
        context.Response.Headers["X-RateLimit-Limit"] = rateLimitPolicy.PermitLimit.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = remainingRequests.ToString();
        context.Response.Headers["X-RateLimit-Reset"] =
            DateTimeOffset.UtcNow.AddMinutes(rateLimitPolicy.WindowMinutes).ToUnixTimeSeconds().ToString();

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Try to get user ID from JWT token first
        var userId = context.User?.FindFirst("sub")?.Value ??
                    context.User?.FindFirst("userId")?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }

        // Fall back to IP address
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // Check for forwarded IP (behind proxy/load balancer)
        if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim() ?? ipAddress;
        }
        else if (context.Request.Headers.ContainsKey("X-Real-IP"))
        {
            ipAddress = context.Request.Headers["X-Real-IP"].FirstOrDefault() ?? ipAddress;
        }

        return $"ip:{ipAddress}";
    }

    private string GetEndpointIdentifier(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        var method = context.Request.Method.ToUpper();

        // Categorize endpoints for rate limiting
        if (path.StartsWith("/api/auth/login") || path.StartsWith("/api/auth/oauth"))
        {
            return "login";
        }

        if (path.StartsWith("/api/auth/register") || path.StartsWith("/api/auth/verify-email"))
        {
            return "registration";
        }

        if (path.Contains("/email") || path.StartsWith("/api/auth/forgot-password"))
        {
            return "email";
        }

        return "api";
    }

    private RateLimitPolicy? GetRateLimitPolicy(string endpoint)
    {
        return endpoint switch
        {
            "login" => _settings.LoginAttempts,
            "registration" => _settings.LoginAttempts, // Reuse login limits for registration
            "email" => _settings.EmailSending,
            "api" => _settings.ApiCalls,
            _ => _settings.ApiCalls
        };
    }

    private class ClientRateLimit
    {
        public List<DateTime> Requests { get; } = new();
    }
}

public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }
}