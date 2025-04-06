using MongoDB.Bson.Serialization.Attributes;

namespace AscendDev.Core.Models.Courses;

public class AdditionalResource
{
    [BsonElement("title")] public string Title { get; set; } = null!;

    [BsonElement("url")] public string Url { get; set; } = null!;
}