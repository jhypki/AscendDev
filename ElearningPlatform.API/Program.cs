using System.Text;
using Dapper;
using ElearningPlatform.API.Middleware;
using ElearningPlatform.Core.Interfaces.Data;
using ElearningPlatform.Core.Interfaces.Services;
using ElearningPlatform.Core.Interfaces.Utils;
using ElearningPlatform.Core.Models.Auth;
using ElearningPlatform.Data;
using ElearningPlatform.Data.Repositories;
using ElearningPlatform.Data.Repositories.Redis;
using ElearningPlatform.Services.Services.Auth;
using ElearningPlatform.Services.Utilities;
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
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtHelper, JwtHelper>();


// Register AuthService as the implementation of IAuthService
builder.Services.AddScoped<IAuthService, AuthService>();

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