using AscendDev.Core.Models.CodeExecution;
using AscendDev.Core.Models.Courses;
using Docker.DotNet.Models;

namespace AscendDev.Core.Interfaces.CodeExecution;

/// <summary>
///     Interface for language-specific code execution strategies
/// </summary>
public interface ILanguageStrategy
{
    /// <summary>
    ///     Check if this strategy supports the given language
    /// </summary>
    bool SupportsLanguage(string language);

    /// <summary>
    ///     Prepare files needed for test execution
    /// </summary>
    Task PrepareTestFilesAsync(string executionDirectory, string userCode, Lesson lesson);

    /// <summary>
    ///     Create Docker container configuration for this language
    /// </summary>
    Task<CreateContainerParameters> CreateContainerConfigAsync(string containerName, string executionDirectory,
        Lesson lesson);

    /// <summary>
    ///     Parse the execution results and convert to TestResult
    /// </summary>
    Task<TestResult> ProcessExecutionResultAsync(string stdout, string stderr, int exitCode, long executionTimeMs,
        string executionDirectory, TestConfig testConfig);
}