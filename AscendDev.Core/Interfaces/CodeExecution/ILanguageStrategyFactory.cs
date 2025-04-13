namespace AscendDev.Core.Interfaces.CodeExecution;

public interface ILanguageStrategyFactory
{
    ILanguageStrategy GetStrategy(string language);
}