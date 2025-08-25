using AscendDev.Core.Models.TestsExecution.KeywordValidation;
using AscendDev.Core.Models.TestsExecution;

namespace AscendDev.Core.Models.CodeExecution;

public class TestResult
{
    public bool Success { get; set; }
    public List<TestCaseResult> TestResults { get; set; }
    public KeywordValidationResult? KeywordValidation { get; set; }
    public PerformanceMetrics? Performance { get; set; }
}