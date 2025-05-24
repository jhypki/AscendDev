using AscendDev.Core.Models.CodeExecution;
using Docker.DotNet.Models;

namespace AscendDev.Core.Interfaces.CodeExecution;

/// <summary>
///     Interface for language-specific code execution strategies in playground
/// </summary>
public interface ILanguageExecutionStrategy
{
    /// <summary>
    ///     Check if this strategy supports the given language
    /// </summary>
    bool SupportsLanguage(string language);

    /// <summary>
    ///     Get the appropriate file name for the source code based on language
    /// </summary>
    string GetSourceFileName(string code);

    /// <summary>
    ///     Create Docker container configuration for this language
    /// </summary>
    CreateContainerParameters CreateContainerConfig(string containerName, string executionDirectory, string language);

    /// <summary>
    ///     Parse the execution results and convert to CodeExecutionResult
    /// </summary>
    Task<CodeExecutionResult> ProcessExecutionResultAsync(string stdout, string stderr, int exitCode, long executionTimeMs,
        string executionDirectory);
}