using MongoDB.Bson.Serialization.Attributes;

namespace AscendDev.Core.Models.Courses;

public class TestCase
{
    // [BsonElement("description")]
    // public string Description { get; set; } = null!; TODO: Add this to the TestCase config
    [BsonElement("input")] public object Input { get; set; } = null!; // Can be any BSON type

    [BsonElement("expected")] public object Expected { get; set; } = null!;
    [BsonElement("description")] public string? Description { get; set; } = null!; // Optional description
}