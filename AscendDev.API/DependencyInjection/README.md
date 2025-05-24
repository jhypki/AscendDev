# Dependency Injection Structure

This folder contains extension methods for organizing dependency injection in the AscendDev application. Each file focuses on a specific category of services, making the codebase more maintainable and easier to understand.

## Extension Classes

### AuthenticationExtensions

Handles JWT authentication configuration.

- `AddJwtAuthentication`: Configures JWT Bearer authentication with settings from configuration.

### CacheExtensions

Configures Redis caching.

- `AddRedisCache`: Sets up Redis cache with connection details from configuration.

### CorsExtensions

Manages Cross-Origin Resource Sharing (CORS) policies.

- `AddCorsPolicy`: Registers CORS policies.
- `UseCorsPolicy`: Applies CORS middleware to the application pipeline.

### DatabaseExtensions

Handles database connections and repositories.

- `AddDatabaseServices`: Registers database connection managers and SQL executors.
- `AddRepositories`: Registers all repository implementations.

### LoggingExtensions

Configures application logging.

- `ConfigureLogging`: Sets up console and debug logging with appropriate filter levels.

### MiddlewareExtensions

Manages custom middleware components.

- `AddCustomMiddleware`: Registers middleware services if needed.
- `UseCustomMiddleware`: Adds custom middleware to the application pipeline.

### MvcExtensions

Configures MVC and API-related services.

- `AddMvcServices`: Sets up controllers, JSON formatting, and API behavior.

### ServiceExtensions

Handles application services and utilities.

- `AddApplicationServices`: Registers core application services.
- `AddUtilities`: Registers utility services.

### SwaggerExtensions

Configures Swagger documentation.

- `AddSwaggerDocumentation`: Registers Swagger services.
- `UseSwaggerDocumentation`: Adds Swagger middleware to the application pipeline.

## Usage

These extension methods are used in `Program.cs` to organize service registration in a clean and maintainable way. To add new services:

1. Identify the appropriate extension class based on the service category.
2. Add your service registration to the relevant extension method.
3. If you need to create a new category, create a new extension class following the same pattern.

Example:

```csharp
// Adding a new repository in DatabaseExtensions.cs
public static IServiceCollection AddRepositories(this IServiceCollection services)
{
    // Existing repositories...
    services.AddScoped<INewRepository, NewRepository>();

    return services;
}
```
