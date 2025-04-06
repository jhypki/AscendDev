using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AscendDev.Core.Models.Courses;

public class Course
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = null!;

    [BsonElement("title")] public string Title { get; set; } = null!;

    [BsonElement("slug")] public string Slug { get; set; } = null!;

    [BsonElement("description")] public string Description { get; set; } = null!;

    [BsonElement("createdAt")] public DateTime CreatedAt { get; set; }

    [BsonElement("language")] public string Language { get; set; } = null!;

    [BsonElement("updatedAt")] public DateTime? UpdatedAt { get; set; }

    [BsonElement("featuredImage")] public string? FeaturedImage { get; set; }

    [BsonElement("tags")] public List<string> Tags { get; set; } = [];

    [BsonElement("lessonSummaries")] public List<LessonSummary> LessonSummaries { get; set; } = [];

    [BsonElement("status")] public string Status { get; set; } = "draft";

    [BsonElement("createdBy")] public string CreatedBy { get; set; } // TODO: change to guid
}