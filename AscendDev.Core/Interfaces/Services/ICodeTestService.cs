using AscendDev.Core.DTOs.CodeExecution;
using AscendDev.Core.Models.CodeExecution;

namespace AscendDev.Core.Interfaces.Services;

public interface ICodeTestService
{
    /// <summary>
    /// Runs tests using the request format that supports both legacy code and template-based submissions
    /// </summary>
    Task<TestResult> RunTestsAsync(RunTestsRequest request, Guid? userId = null);
}