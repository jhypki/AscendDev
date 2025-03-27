using System.Text;

namespace ElearningPlatform.API.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        await LogRequest(context);

        await next(context);
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
}