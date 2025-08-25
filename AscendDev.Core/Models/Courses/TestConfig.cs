using System.Text.Json.Serialization;
using AscendDev.Core.Models.TestsExecution.KeywordValidation;

namespace AscendDev.Core.Models.Courses;

public class TestConfig
{
    [JsonPropertyName("timeoutMs")] public int TimeoutMs { get; set; }

    [JsonPropertyName("memoryLimitMb")] public int MemoryLimitMb { get; set; }

    [JsonPropertyName("testTemplate")] public string TestTemplate { get; set; } = null!;

    [JsonPropertyName("testCases")] public List<TestCase> TestCases { get; set; } = [];

    [JsonPropertyName("keywordRequirements")] public List<KeywordRequirement> KeywordRequirements { get; set; } = [];
}