using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AscendDev.API.DependencyInjection;

public static class LoggingExtensions
{
    public static ILoggingBuilder ConfigureLogging(this ILoggingBuilder logging)
    {
        // Configure logging
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddDebug();
        logging.SetMinimumLevel(LogLevel.Information);
        logging.AddFilter("Microsoft", LogLevel.Warning);
        logging.AddFilter("System", LogLevel.Warning);

        return logging;
    }
}