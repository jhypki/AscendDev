using MongoDB.Bson.Serialization.Attributes;

namespace AscendDev.Core.Models.Courses;

public class Dependency
{
    [BsonElement("name")] public string Name { get; set; } = null!;

    [BsonElement("version")] public string Version { get; set; } = null!;
}