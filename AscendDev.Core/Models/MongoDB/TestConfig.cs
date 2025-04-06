using MongoDB.Bson.Serialization.Attributes;

namespace AscendDev.Core.Models.Courses;

public class TestConfig
{
    [BsonElement("framework")] public string Framework { get; set; } = null!;

    [BsonElement("timeoutMs")] public int TimeoutMs { get; set; }

    [BsonElement("memoryLimitMb")] public int MemoryLimitMb { get; set; }

    [BsonElement("testTemplate")] public string TestTemplate { get; set; } = null!;

    [BsonElement("testCases")] public List<TestCase> TestCases { get; set; } = [];

    [BsonElement("mainFunction")] public string MainFunction { get; set; } = null!;

    [BsonElement("dependencies")] public List<Dependency> Dependencies { get; set; } = [];
}