using AscendDev.Core.CodeExecution;
using AscendDev.Core.CodeExecution.Sanitizers;
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
        services.AddTransient<ILanguageStrategy, GoStrategy>();
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
        services.AddTransient<ILanguageExecutionStrategy, GoExecutionStrategy>();
        services.AddTransient<ILanguageExecutionStrategy, PythonExecutionStrategy>();

        // Register the factory
        services.AddTransient<ILanguageExecutionStrategyFactory, LanguageExecutionStrategyFactory>();
    }

    /// <summary>
    ///     Register all code sanitizers and the factory
    /// </summary>
    public static void AddCodeSanitizers(this IServiceCollection services)
    {
        // Register JavaScript sanitizer first (needed by TypeScript sanitizer)
        services.AddTransient<JavaScriptSanitizer>();

        // Register all sanitizers
        services.AddTransient<ICodeSanitizer, PythonSanitizer>();
        services.AddTransient<ICodeSanitizer, GoSanitizer>();
        services.AddTransient<ICodeSanitizer, JavaScriptSanitizer>();
        services.AddTransient<ICodeSanitizer, TypeScriptSanitizer>();

        // Register the factory
        services.AddTransient<ICodeSanitizerFactory, CodeSanitizerFactory>();
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

        // Register code sanitizers
        services.AddCodeSanitizers();

        // Register executors
        services.AddTransient<ITestsExecutor, DockerTestsExecutor>();
        services.AddTransient<ICodeExecutor, DockerCodeExecutor>();
    }
}