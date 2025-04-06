using MongoDB.Bson.Serialization.Attributes;

namespace AscendDev.Core.Models.Courses;

public class LessonSummary
{
    [BsonElement("lessonId")] public string LessonId { get; set; } = null!;

    [BsonElement("title")] public string Title { get; set; } = null!;

    [BsonElement("slug")] public string Slug { get; set; } = null!;

    [BsonElement("order")] public int Order { get; set; }
}