using AscendDev.Core.Interfaces.CodeExecution;

namespace AscendDev.Core.TestsExecution;

public class LanguageTestStrategyFactory(IEnumerable<ILanguageTestStrategy> strategies) : ILanguageTestStrategyFactory
{
    private readonly IEnumerable<ILanguageTestStrategy> _strategies =
        strategies ?? throw new ArgumentNullException(nameof(strategies));

    public ILanguageTestStrategy GetStrategy(string language)
    {
        if (string.IsNullOrEmpty(language))
            throw new ArgumentException("Language cannot be null or empty", nameof(language));

        var strategy = _strategies.FirstOrDefault(s => s.SupportsLanguage(language));

        if (strategy == null)
            throw new NotSupportedException($"Language '{language}' is not supported");

        return strategy;
    }
}