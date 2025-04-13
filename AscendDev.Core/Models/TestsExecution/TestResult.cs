namespace AscendDev.Core.Models.CodeExecution;

public class TestResult
{
    public bool Success { get; set; }
    public List<TestCaseResult> TestResults { get; set; }
}