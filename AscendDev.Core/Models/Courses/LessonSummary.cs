using System.Text.Json.Serialization;

namespace AscendDev.Core.Models.Courses;

public class LessonSummary
{
    [JsonPropertyName("id")] public string Id { get; set; } = null!;

    [JsonPropertyName("title")] public string Title { get; set; } = null!;

    [JsonPropertyName("slug")] public string Slug { get; set; } = null!;

    [JsonPropertyName("order")] public int Order { get; set; }
}