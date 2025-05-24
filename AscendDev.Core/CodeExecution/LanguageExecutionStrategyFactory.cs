using AscendDev.Core.Interfaces.CodeExecution;

namespace AscendDev.Core.CodeExecution;

public class LanguageExecutionStrategyFactory(IEnumerable<ILanguageExecutionStrategy> strategies) : ILanguageExecutionStrategyFactory
{
    private readonly IEnumerable<ILanguageExecutionStrategy> _strategies =
        strategies ?? throw new ArgumentNullException(nameof(strategies));

    public ILanguageExecutionStrategy GetStrategy(string language)
    {
        if (string.IsNullOrEmpty(language))
            throw new ArgumentException("Language cannot be null or empty", nameof(language));

        var strategy = _strategies.FirstOrDefault(s => s.SupportsLanguage(language));

        if (strategy == null)
            throw new NotSupportedException($"Language '{language}' is not supported");

        return strategy;
    }
}