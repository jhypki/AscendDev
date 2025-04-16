using System.Text.Json.Serialization;

namespace AscendDev.Core.Models.Courses;

public class TestConfig
{
    [JsonPropertyName("timeoutMs")] public int TimeoutMs { get; set; }

    [JsonPropertyName("memoryLimitMb")] public int MemoryLimitMb { get; set; }

    [JsonPropertyName("testTemplate")] public string TestTemplate { get; set; } = null!;

    [JsonPropertyName("testCases")] public List<TestCase> TestCases { get; set; } = [];
}