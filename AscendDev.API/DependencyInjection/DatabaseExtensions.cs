using AscendDev.Core.Interfaces.Data;
using AscendDev.Data;
using AscendDev.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using StackExchange.Redis;

namespace AscendDev.API.DependencyInjection;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services)
    {
        // Register database connection managers
        services.AddSingleton<IConnectionManager<NpgsqlConnection>, PostgresqlConnectionManager>();
        services.AddSingleton<IConnectionManager<IDatabase>, RedisConnectionManager>();

        // Register DapperSqlExecutor and map ISqlExecutor
        services.AddScoped(typeof(DapperSqlExecutor<>));
        services.AddScoped<ISqlExecutor>(sp => sp.GetRequiredService<DapperSqlExecutor<NpgsqlConnection>>());

        // Setup Dapper type handlers
        DapperConfig.SetupTypeHandlers();

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<ILessonRepository, LessonRepository>();
        services.AddScoped<IUserProgressRepository, UserProgressRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<ISubmissionRepository, SubmissionRepository>();
        services.AddScoped<IUserSettingsRepository, UserSettingsRepository>();
        services.AddScoped<IUserLessonCodeRepository, UserLessonCodeRepository>();

        // Register admin repositories
        services.AddScoped<IUserManagementRepository, UserManagementRepository>();
        services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

        // Register social repositories
        services.AddScoped<IDiscussionRepository, DiscussionRepository>();
        services.AddScoped<IDiscussionReplyRepository, DiscussionReplyRepository>();
        services.AddScoped<IDiscussionLikeRepository, DiscussionLikeRepository>();
        services.AddScoped<ICodeReviewRepository, CodeReviewRepository>();
        services.AddScoped<ICodeReviewCommentRepository, CodeReviewCommentRepository>();

        return services;
    }
}