using AscendDev.Core.Models.CodeExecution;

namespace AscendDev.Core.Interfaces.Services;

public interface ICodeExecutionService
{
    Task<CodeExecutionResult> ExecuteCodeAsync(string language, string code);
}