using AscendDev.Core.Models.CodeExecution;

namespace AscendDev.Core.Interfaces.CodeExecution;

public interface ICodeExecutor
{
    Task<CodeExecutionResult> ExecuteAsync(string language, string code);
}