using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AscendDev.Core.Models.Courses;

public class Lesson
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = null!;

    [BsonElement("courseId")] public string CourseId { get; set; } = null!;

    [BsonElement("title")] public string Title { get; set; } = null!;

    [BsonElement("slug")] public string Slug { get; set; } = null!;

    [BsonElement("content")] public string Content { get; set; } = null!;

    [BsonElement("language")] public string Language { get; set; } = null!;

    [BsonElement("template")] public string Template { get; set; } = null!;

    [BsonElement("createdAt")] public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")] public DateTime UpdatedAt { get; set; }

    [BsonElement("order")] public int Order { get; set; }

    // [BsonElement("courseId")]
    // public string Status { get; set; } = "Draft";
    [BsonElement("testConfig")] public TestConfig TestConfig { get; set; } = null!;

    [BsonElement("additionalResources")] public List<AdditionalResource> AdditionalResources { get; set; } = [];

    [BsonElement("tags")] public List<string> Tags { get; set; } = [];
}