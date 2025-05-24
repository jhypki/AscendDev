using AscendDev.Core.CodeExecution;
using AscendDev.Core.Interfaces.CodeExecution;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Interfaces.Utils;
using AscendDev.Core.TestsExecution;
using AscendDev.Services.Services;
using AscendDev.Services.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace AscendDev.API.DependencyInjection;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<ILessonService, LessonService>();
        services.AddScoped<ICachingService, CachingService>();
        services.AddScoped<ICodeTestService, CodeTestService>();

        // Register utilities
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtHelper, JwtHelper>();
        services.AddScoped<ITestsExecutor, DockerTestsExecutor>();

        // Register language strategies
        services.AddLanguageStrategies();

        return services;
    }

    public static IServiceCollection AddUtilities(this IServiceCollection services)
    {
        // Register singleton utilities
        services.AddSingleton<JwtHelper>();
        services.AddSingleton<PasswordHasher>();

        return services;
    }
}