using AscendDev.Core.CodeExecution.Strategies;
using AscendDev.Core.Interfaces.CodeExecution;
using Microsoft.Extensions.DependencyInjection;

namespace AscendDev.Core.CodeExecution;

public static class CodeExecutionServiceExtensions
{
    /// <summary>
    ///     Register all language strategies and the factory
    /// </summary>
    public static void AddLanguageStrategies(this IServiceCollection services)
    {
        // Register all strategies
        services.AddTransient<ILanguageStrategy, TypeScriptStrategy>();
        // services.AddTransient<ILanguageStrategy, JavaScriptStrategy>();
        services.AddTransient<ILanguageStrategy, CSharpStrategy>();
        services.AddTransient<ILanguageStrategy, PythonStrategy>();

        // Register the factory
        services.AddTransient<ILanguageStrategyFactory, LanguageStrategyFactory>();
    }
}