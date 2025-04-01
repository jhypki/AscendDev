namespace AscendDev.Core.Models.Courses;

public class TestConfig
{
    public string Framework { get; set; } = null!;
    public int TimeoutMs { get; set; }
    public int MemoryLimitMb { get; set; }
    public string TestTemplate { get; set; } = null!;
    public List<TestCase> TestCases { get; set; } = [];
    public List<Dependency> Dependencies { get; set; } = [];
}