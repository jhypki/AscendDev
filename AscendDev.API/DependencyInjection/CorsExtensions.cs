using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AscendDev.API.DependencyInjection;

public static class CorsExtensions
{
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        // Configure CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                // Get allowed origins from environment variable or use defaults
                var allowedOrigins = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS")?.Split(',')
                    ?? new[] {
                        "http://localhost:5173",
                        "http://localhost:3000",
                        "https://localhost:5173",
                        "https://localhost:3000"
                    };

                // Always use specific origins to support credentials
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (environment == "Development")
                {
                    // In development, include all common development origins
                    var devOrigins = allowedOrigins.Concat(new[] {
                        "http://frontend:3000",
                        "http://api:5171",
                        "http://localhost:8080",
                        "http://127.0.0.1:3000",
                        "http://127.0.0.1:5173"
                    }).ToArray();

                    policy.WithOrigins(devOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                }
                else
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                }
            });
        });

        return services;
    }

    public static IApplicationBuilder UseCorsPolicy(this IApplicationBuilder app)
    {
        app.UseCors("AllowAll");

        return app;
    }
}