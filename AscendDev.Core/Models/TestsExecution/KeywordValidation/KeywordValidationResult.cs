namespace AscendDev.Core.Models.TestsExecution.KeywordValidation;

public class KeywordValidationResult
{
    public bool IsValid { get; set; }
    public List<KeywordValidationError> Errors { get; set; } = [];
    public List<KeywordMatch> Matches { get; set; } = [];
    public string ValidationMessage { get; set; } = string.Empty;
}

public class KeywordValidationError
{
    public string Keyword { get; set; } = null!;
    public string ErrorMessage { get; set; } = null!;
    public KeywordErrorType ErrorType { get; set; }
    public int ExpectedOccurrences { get; set; }
    public int ActualOccurrences { get; set; }
}

public class KeywordMatch
{
    public string Keyword { get; set; } = null!;
    public int LineNumber { get; set; }
    public int ColumnStart { get; set; }
    public int ColumnEnd { get; set; }
    public string MatchedText { get; set; } = null!;
}

public enum KeywordErrorType
{
    Missing,
    TooFew,
    TooMany,
    CaseMismatch
}