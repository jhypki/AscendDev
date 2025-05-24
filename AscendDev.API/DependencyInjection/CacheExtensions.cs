using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AscendDev.API.DependencyInjection;

public static class CacheExtensions
{
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            var redisHost = configuration.GetSection("Redis")["Host"] ?? "localhost";
            var redisPort = configuration.GetSection("Redis")["Port"] ?? "6379";
            var redisPassword = configuration.GetSection("Redis")["Password"] ?? string.Empty;

            options.Configuration = $"{redisHost}:{redisPort},abortConnect=false,password={redisPassword}";
            options.InstanceName = "AscendDev_";
        });

        return services;
    }
}