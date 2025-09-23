using AscendDev.API.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ConfigureLogging();

// Add MVC and API configuration
builder.Services.AddMvcServices();

// Add Swagger documentation
builder.Services.AddSwaggerDocumentation();

// Add JWT authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add Redis cache
builder.Services.AddRedisCache(builder.Configuration);

// Add database services
builder.Services.AddDatabaseServices();
builder.Services.AddRepositories();

// Add application services
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddUtilities();

// Add CORS policy
builder.Services.AddCorsPolicy();

// Add SignalR
builder.Services.AddSignalRServices();

// Add custom middleware
builder.Services.AddCustomMiddleware();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwaggerDocumentation(app.Environment);

app.UseHttpsRedirection();

// Add custom middleware
app.UseCustomMiddleware();

// Add CORS
app.UseCorsPolicy();

// Add authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map SignalR hubs
app.UseSignalRHubs();

app.Run();