using System.Text;

namespace AscendDev.API.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        await LogRequest(context);

        await next(context);

        await LogResponse(context);
    }

    private async Task LogRequest(HttpContext context)
    {
        try
        {
            var request = context.Request;

            var logBuilder = new StringBuilder();
            logBuilder.AppendLine($"HTTP {request.Method} {request.Path}{request.QueryString}");

            logBuilder.AppendLine("Headers:");
            foreach (var header in request.Headers) logBuilder.AppendLine($"{header.Key}: {header.Value}");

            if (request.Body.CanRead && request.ContentLength > 0)
            {
                request.EnableBuffering();

                using var reader = new StreamReader(
                    request.Body,
                    Encoding.UTF8,
                    false,
                    leaveOpen: true);

                var body = await reader.ReadToEndAsync();
                logBuilder.AppendLine("Body:");
                logBuilder.AppendLine(body);

                request.Body.Position = 0;
            }

            logger.LogInformation("Incoming Request:\n{RequestDetails}", logBuilder.ToString());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error logging request");
        }
    }

    private async Task LogResponse(HttpContext context)
    {
        try
        {
            var response = context.Response;

            var logBuilder = new StringBuilder();
            logBuilder.AppendLine($"HTTP {response.StatusCode}");

            logBuilder.AppendLine("Headers:");
            foreach (var header in response.Headers) logBuilder.AppendLine($"{header.Key}: {header.Value}");

            if (response.Body.CanRead && response.ContentLength > 0)
            {
                response.Body.Position = 0;

                using var reader = new StreamReader(
                    response.Body,
                    Encoding.UTF8,
                    false,
                    leaveOpen: true);

                var body = await reader.ReadToEndAsync();
                logBuilder.AppendLine("Body:");
                logBuilder.AppendLine(body);

                response.Body.Position = 0;
            }

            logger.LogInformation("Outgoing Response:\n{ResponseDetails}", logBuilder.ToString());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error logging response");
        }
    }
}