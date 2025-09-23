using AscendDev.API.Hubs;
using Microsoft.Extensions.DependencyInjection;

namespace AscendDev.API.DependencyInjection;

public static class SignalRExtensions
{
    public static IServiceCollection AddSignalRServices(this IServiceCollection services)
    {
        services.AddSignalR(options =>
        {
            // Configure SignalR options
            options.EnableDetailedErrors = true;
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            options.HandshakeTimeout = TimeSpan.FromSeconds(15);
        });

        return services;
    }

    public static WebApplication UseSignalRHubs(this WebApplication app)
    {
        app.MapHub<NotificationHub>("/hubs/notifications");
        return app;
    }
}