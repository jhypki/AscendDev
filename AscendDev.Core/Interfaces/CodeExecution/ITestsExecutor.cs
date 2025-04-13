using AscendDev.Core.Models.CodeExecution;
using AscendDev.Core.Models.Courses;

namespace AscendDev.Core.Interfaces.CodeExecution;

public interface ITestsExecutor
{
    Task<TestResult> ExecuteAsync(string userCode, Lesson lesson);
}