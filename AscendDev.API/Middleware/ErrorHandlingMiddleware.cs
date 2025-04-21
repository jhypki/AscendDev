using AscendDev.Core.DTOs;
using AscendDev.Core.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AscendDev.API.Middleware;

public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        ApiResponse<object?> response;

        logger.LogError(exception, exception.Message);

        switch (exception)
        {
            case ValidationException validationEx:
                context.Response.StatusCode = validationEx.StatusCode;
                response = ApiResponse<object>.ErrorResponse(
                    validationEx.Message,
                    validationEx.Errors.SelectMany(e => e.Value).ToList()
                );
                break;

            case NotFoundException notFoundEx:
                context.Response.StatusCode = notFoundEx.StatusCode;
                response = ApiResponse<object>.ErrorResponse(
                    notFoundEx.Message,
                    notFoundEx.Details != null ? [notFoundEx.Details] : null
                );
                break;

            case UnauthorizedException unauthorizedEx:
                context.Response.StatusCode = unauthorizedEx.StatusCode;
                response = ApiResponse<object>.ErrorResponse(
                    unauthorizedEx.Message,
                    unauthorizedEx.Details != null ? [unauthorizedEx.Details] : null
                );
                break;

            case ForbiddenException forbiddenEx:
                context.Response.StatusCode = forbiddenEx.StatusCode;
                response = ApiResponse<object>.ErrorResponse(
                    forbiddenEx.Message,
                    forbiddenEx.Details != null ? [forbiddenEx.Details] : null
                );
                break;

            case ConflictException conflictEx:
                context.Response.StatusCode = conflictEx.StatusCode;
                response = ApiResponse<object>.ErrorResponse(
                    conflictEx.Message,
                    conflictEx.Details != null ? [conflictEx.Details] : null
                );
                break;

            case ApiException apiEx:
                context.Response.StatusCode = apiEx.StatusCode;
                response = ApiResponse<object>.ErrorResponse(
                    apiEx.Message,
                    apiEx.Details != null ? [apiEx.Details] : null
                );
                break;

            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response = ApiResponse<object>.ErrorResponse("An unexpected error occurred");
                break;
        }

        var settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            },
            Formatting = Formatting.Indented
        };

        var jsonResponse = JsonConvert.SerializeObject(response, settings);
        return context.Response.WriteAsync(jsonResponse);
    }
}