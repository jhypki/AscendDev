using AscendDev.Core.CodeExecution;
using AscendDev.Core.CodeExecution.Sanitizers;
using AscendDev.Core.CodeExecution.Strategies;
using AscendDev.Core.Interfaces.CodeExecution;
using AscendDev.Core.TestsExecution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace AscendDev.Core.Test.TestsExecution;

[TestFixture]
public class LanguageStrategiesExtensionsTests
{
    private IServiceCollection _services;

    [SetUp]
    public void Setup()
    {
        _services = new ServiceCollection();

        // Register mock loggers for all strategy types
        _services.AddSingleton(new Mock<ILogger<TypeScriptStrategy>>().Object);
        _services.AddSingleton(new Mock<ILogger<CSharpStrategy>>().Object);
        _services.AddSingleton(new Mock<ILogger<PythonStrategy>>().Object);
        _services.AddSingleton(new Mock<ILogger<TypeScriptExecutionStrategy>>().Object);
        _services.AddSingleton(new Mock<ILogger<JavaScriptExecutionStrategy>>().Object);
        _services.AddSingleton(new Mock<ILogger<CSharpExecutionStrategy>>().Object);
        _services.AddSingleton(new Mock<ILogger<PythonExecutionStrategy>>().Object);
        _services.AddSingleton(new Mock<ILogger<DockerTestsExecutor>>().Object);
        _services.AddSingleton(new Mock<ILogger<DockerCodeExecutor>>().Object);
    }

    [Test]
    public void AddLanguageStrategies_RegistersAllStrategiesAndFactory()
    {
        // Act
        _services.AddLanguageStrategies();
        var provider = _services.BuildServiceProvider();

        // Assert
        // Verify factory is registered
        var factory = provider.GetService<ILanguageStrategyFactory>();
        Assert.That(factory, Is.Not.Null);
        Assert.That(factory, Is.TypeOf<LanguageStrategyFactory>());

        // Verify strategies are registered
        var strategies = provider.GetServices<ILanguageStrategy>().ToList();
        Assert.That(strategies, Has.Count.GreaterThanOrEqualTo(3)); // At least TypeScript, CSharp, Python

        Assert.That(strategies, Has.Some.TypeOf<TypeScriptStrategy>());
        Assert.That(strategies, Has.Some.TypeOf<CSharpStrategy>());
        Assert.That(strategies, Has.Some.TypeOf<PythonStrategy>());
    }

    [Test]
    public void AddLanguageExecutionStrategies_RegistersAllStrategiesAndFactory()
    {
        // Act
        _services.AddLanguageExecutionStrategies();
        var provider = _services.BuildServiceProvider();

        // Assert
        // Verify factory is registered
        var factory = provider.GetService<ILanguageExecutionStrategyFactory>();
        Assert.That(factory, Is.Not.Null);
        Assert.That(factory, Is.TypeOf<LanguageExecutionStrategyFactory>());

        // Verify strategies are registered
        var strategies = provider.GetServices<ILanguageExecutionStrategy>().ToList();
        Assert.That(strategies, Has.Count.GreaterThanOrEqualTo(4)); // TypeScript, JavaScript, CSharp, Python

        Assert.That(strategies, Has.Some.TypeOf<TypeScriptExecutionStrategy>());
        Assert.That(strategies, Has.Some.TypeOf<JavaScriptExecutionStrategy>());
        Assert.That(strategies, Has.Some.TypeOf<CSharpExecutionStrategy>());
        Assert.That(strategies, Has.Some.TypeOf<PythonExecutionStrategy>());
    }

    [Test]
    public void AddCodeSanitizers_RegistersAllSanitizersAndFactory()
    {
        // Act
        _services.AddCodeSanitizers();
        var provider = _services.BuildServiceProvider();

        // Assert
        // Verify factory is registered
        var factory = provider.GetService<ICodeSanitizerFactory>();
        Assert.That(factory, Is.Not.Null);
        Assert.That(factory, Is.TypeOf<CodeSanitizerFactory>());

        // Verify sanitizers are registered
        var sanitizers = provider.GetServices<ICodeSanitizer>().ToList();
        Assert.That(sanitizers, Has.Count.GreaterThanOrEqualTo(4)); // Python, CSharp, JavaScript, TypeScript

        Assert.That(sanitizers, Has.Some.TypeOf<PythonSanitizer>());
        Assert.That(sanitizers, Has.Some.TypeOf<CSharpSanitizer>());
        Assert.That(sanitizers, Has.Some.TypeOf<JavaScriptSanitizer>());
        Assert.That(sanitizers, Has.Some.TypeOf<TypeScriptSanitizer>());

        // Verify JavaScript sanitizer is registered directly (needed by TypeScript sanitizer)
        var jsSanitizer = provider.GetService<JavaScriptSanitizer>();
        Assert.That(jsSanitizer, Is.Not.Null);
    }

    [Test]
    public void AddCodeExecutionServices_RegistersAllServices()
    {
        // Act
        _services.AddCodeExecutionServices();
        var provider = _services.BuildServiceProvider();

        // Assert
        // Verify language strategies are registered
        var strategyFactory = provider.GetService<ILanguageStrategyFactory>();
        Assert.That(strategyFactory, Is.Not.Null);

        // Verify execution strategies are registered
        var executionStrategyFactory = provider.GetService<ILanguageExecutionStrategyFactory>();
        Assert.That(executionStrategyFactory, Is.Not.Null);

        // Verify sanitizers are registered
        var sanitizerFactory = provider.GetService<ICodeSanitizerFactory>();
        Assert.That(sanitizerFactory, Is.Not.Null);

        // Verify executors are registered
        var testsExecutor = provider.GetService<ITestsExecutor>();
        Assert.That(testsExecutor, Is.Not.Null);
        Assert.That(testsExecutor, Is.TypeOf<DockerTestsExecutor>());

        var codeExecutor = provider.GetService<ICodeExecutor>();
        Assert.That(codeExecutor, Is.Not.Null);
        Assert.That(codeExecutor, Is.TypeOf<DockerCodeExecutor>());
    }
}