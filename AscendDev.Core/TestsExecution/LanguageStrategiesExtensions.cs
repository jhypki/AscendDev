using AscendDev.Core.CodeExecution;
using AscendDev.Core.CodeExecution.Strategies;
using AscendDev.Core.Interfaces.CodeExecution;
using Microsoft.Extensions.DependencyInjection;

namespace AscendDev.Core.TestsExecution;

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

    /// <summary>
    ///     Register all language execution strategies and the factory
    /// </summary>
    public static void AddLanguageExecutionStrategies(this IServiceCollection services)
    {
        // Register all execution strategies
        services.AddTransient<ILanguageExecutionStrategy, TypeScriptExecutionStrategy>();
        services.AddTransient<ILanguageExecutionStrategy, JavaScriptExecutionStrategy>();
        services.AddTransient<ILanguageExecutionStrategy, CSharpExecutionStrategy>();
        services.AddTransient<ILanguageExecutionStrategy, PythonExecutionStrategy>();

        // Register the factory
        services.AddTransient<ILanguageExecutionStrategyFactory, LanguageExecutionStrategyFactory>();
    }

    /// <summary>
    ///     Register all code execution services
    /// </summary>
    public static void AddCodeExecutionServices(this IServiceCollection services)
    {
        // Register language strategies
        services.AddLanguageStrategies();

        // Register execution strategies
        services.AddLanguageExecutionStrategies();

        // Register executors
        services.AddTransient<ITestsExecutor, DockerTestsExecutor>();
        services.AddTransient<ICodeExecutor, DockerCodeExecutor>();
    }
}