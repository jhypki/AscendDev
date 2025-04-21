using System.Text.Json.Serialization;

namespace AscendDev.Core.Models.Courses;

public class AdditionalResource
{
    [JsonPropertyName("url")] public string Url { get; set; } = null!;

    [JsonPropertyName("title")] public string Title { get; set; } = null!;
}