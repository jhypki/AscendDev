using AscendDev.Core.Constants;
using AscendDev.Core.Interfaces.CodeExecution;
using AscendDev.Core.Models.CodeExecution;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;

namespace AscendDev.Core.CodeExecution.Strategies;

public class TypeScriptExecutionStrategy(ILogger<TypeScriptExecutionStrategy> logger) : ILanguageExecutionStrategy
{
    private readonly ILogger<TypeScriptExecutionStrategy> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public bool SupportsLanguage(string language)
    {
        return language.Equals(SupportedLanguages.TypeScript, StringComparison.OrdinalIgnoreCase);
    }

    public string GetSourceFileName(string code)
    {
        return "index.ts";
    }

    public CreateContainerParameters CreateContainerConfig(string containerName, string executionDirectory, string language)
    {
        var config = new CreateContainerParameters
        {
            Image = DockerImages.TypeScriptRunner,
            Name = containerName,
            HostConfig = new HostConfig
            {
                Memory = 128 * 1024 * 1024, // 128 MB
                MemorySwap = 128 * 1024 * 1024, // Disable swap
                AutoRemove = false,
                Binds = new[] { $"{executionDirectory}:/app/code" } // Mount the code directory
            },
            WorkingDir = "/app",
            Cmd = new[] { "sh", "-c", "/app/run-code.sh" },
            Tty = false,
            AttachStdout = true,
            AttachStderr = true,
            User = "root"
        };

        return config;
    }

    public async Task<CodeExecutionResult> ProcessExecutionResultAsync(string stdout, string stderr, int exitCode,
        long executionTimeMs, string executionDirectory)
    {
        var result = new CodeExecutionResult
        {
            Success = exitCode == 0,
            Stdout = stdout,
            Stderr = stderr,
            ExitCode = exitCode,
            ExecutionTimeMs = executionTimeMs
        };

        // Check if there's a compilation output file
        var compilationOutputPath = Path.Combine(executionDirectory, "compilation.txt");
        if (File.Exists(compilationOutputPath))
        {
            result.CompilationOutput = await File.ReadAllTextAsync(compilationOutputPath);
        }

        _logger.LogInformation("TypeScript code execution complete. Success: {Success}, ExitCode: {ExitCode}, ExecutionTime: {ExecutionTime}ms",
            result.Success, result.ExitCode, result.ExecutionTimeMs);

        return result;
    }
}