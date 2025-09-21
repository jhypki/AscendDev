using AscendDev.Core.CodeExecution;
using AscendDev.Core.Interfaces.CodeExecution;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Interfaces.TestsExecution;
using AscendDev.Core.Interfaces.Utils;
using AscendDev.Core.Models.Configuration;
using AscendDev.Core.TestsExecution;
using AscendDev.Core.TestsExecution.KeywordValidation;
using AscendDev.Data.Repositories;
using AscendDev.Services.Services;
using AscendDev.Services.Services.OAuth;
using AscendDev.Services.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace AscendDev.API.DependencyInjection;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register HttpClient first (required by OAuth providers)
        services.AddHttpClient();

        // Configure OAuth settings
        services.Configure<OAuthSettings>(configuration.GetSection(OAuthSettings.SectionName));

        // Register OAuth providers (after HttpClient)
        services.AddScoped<IOAuthProvider, GoogleOAuthProvider>();
        services.AddScoped<IOAuthProvider, GitHubOAuthProvider>();

        // Register services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOAuthService, OAuthService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<ILessonService, LessonService>();
        services.AddScoped<ICachingService, CachingService>();
        services.AddScoped<ICodeTestService, CodeTestService>();
        services.AddScoped<ICodeExecutionService, CodeExecutionService>();
        services.AddScoped<IUserProgressService, UserProgressService>();
        services.AddScoped<ISubmissionService, SubmissionService>();
        services.AddScoped<IUserSettingsService, UserSettingsService>();

        // Register admin services
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();

        // Register social services
        services.AddScoped<IDiscussionService, DiscussionService>();
        services.AddScoped<IDiscussionReplyService, DiscussionReplyService>();
        services.AddScoped<ICodeReviewService, CodeReviewService>();
        services.AddScoped<ICodeReviewCommentService, CodeReviewCommentService>();

        // Register utilities
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtHelper, JwtHelper>();

        // Register executors
        services.AddScoped<ITestsExecutor, DockerTestsExecutor>();
        services.AddScoped<ICodeExecutor, DockerCodeExecutor>();

        // Register keyword validation service
        services.AddScoped<IKeywordValidationService, KeywordValidationService>();

        // Register code execution services
        services.AddCodeExecutionServices();

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