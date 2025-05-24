using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AscendDev.API.DependencyInjection;

public static class MvcExtensions
{
    public static IServiceCollection AddMvcServices(this IServiceCollection services)
    {
        // Configure MVC
        services.AddControllers(options => { }).AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };
            options.SerializerSettings.Formatting = Formatting.Indented;
        });

        // Configure Dapper naming convention
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        // Configure API behavior
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true; // Disable automatic 400 response
        });

        // Add HttpContextAccessor
        services.AddHttpContextAccessor();

        return services;
    }
}