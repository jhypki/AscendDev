using AscendDev.Core.Models.CodeExecution;

namespace AscendDev.Core.Interfaces.Services;

public interface ICodeTestService
{
    Task<TestResult> RunTestsAsync(string lessonId, string userCode, Guid? userId = null);
}