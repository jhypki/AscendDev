namespace AscendDev.Core.Interfaces.CodeExecution;

public interface ILanguageTestStrategyFactory
{
    ILanguageTestStrategy GetStrategy(string language);
}