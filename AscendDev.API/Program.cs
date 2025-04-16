using System.Text;
using AscendDev.API.Middleware;
using AscendDev.Core.CodeExecution;
using AscendDev.Core.Interfaces.CodeExecution;
using AscendDev.Core.Interfaces.Data;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Interfaces.Utils;
using AscendDev.Core.Models.Auth;
using AscendDev.Core.TestsExecution;
using AscendDev.Data;
using AscendDev.Data.Repositories;
using AscendDev.Services.Services;
using AscendDev.Services.Utilities;
using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Npgsql;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("System", LogLevel.Warning);

// Add services to the container
builder.Services.AddControllers(options => { }).AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ContractResolver = new DefaultContractResolver
    {
        NamingStrategy = new SnakeCaseNamingStrategy()
    };
    options.SerializerSettings.Formatting = Formatting.Indented;
});
DefaultTypeMap.MatchNamesWithUnderscores = true;
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true; // Disable automatic 400 response
});

// Add Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();

// Bind JwtSettings from configuration
var jwtSettings = new JwtSettings();
builder.Configuration.GetSection("JwtSettings").Bind(jwtSettings);
builder.Services.AddSingleton(jwtSettings);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "AscendDev_";
});

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
    };
});

// Register database connection managers
builder.Services.AddSingleton<IConnectionManager<NpgsqlConnection>, PostgresqlConnectionManager>();
builder.Services.AddSingleton<IConnectionManager<IDatabase>, RedisConnectionManager>();

// Register DapperSqlExecutor and map ISqlExecutor
builder.Services.AddScoped(typeof(DapperSqlExecutor<>));
builder.Services.AddScoped<ISqlExecutor>(sp => sp.GetRequiredService<DapperSqlExecutor<NpgsqlConnection>>());

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtHelper, JwtHelper>();
builder.Services.AddScoped<ITestsExecutor, DockerTestsExecutor>();
DapperConfig.SetupTypeHandlers();

// Register AuthService as the implementation of IAuthService
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<ICachingService, CachingService>();
builder.Services.AddScoped<ICodeTestService, CodeTestService>();
builder.Services.AddLanguageStrategies();

// Register utilities
builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddSingleton<PasswordHasher>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add custom middleware
app.UseRequestLogging();
app.UseErrorHandling();

// Add CORS
app.UseCors("AllowAll");

// Add authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();