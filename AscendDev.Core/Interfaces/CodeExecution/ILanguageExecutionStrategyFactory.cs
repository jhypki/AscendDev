namespace AscendDev.Core.Interfaces.CodeExecution;

public interface ILanguageExecutionStrategyFactory
{
    ILanguageExecutionStrategy GetStrategy(string language);
}