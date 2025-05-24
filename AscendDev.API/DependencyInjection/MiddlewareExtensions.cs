using AscendDev.API.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AscendDev.API.DependencyInjection;

public static class MiddlewareExtensions
{
    public static IServiceCollection AddCustomMiddleware(this IServiceCollection services)
    {
        // Register any middleware services if needed

        return services;
    }

    public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder app)
    {
        // Add custom middleware
        app.UseRequestLogging();
        app.UseErrorHandling();

        return app;
    }
}