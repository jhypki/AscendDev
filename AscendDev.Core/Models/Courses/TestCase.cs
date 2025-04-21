using System.Text.Json.Serialization;

namespace AscendDev.Core.Models.Courses;

public class TestCase
{
    [JsonPropertyName("input")] public object Input { get; set; } = null!;

    [JsonPropertyName("expected")] public object Expected { get; set; } = null!;

    [JsonPropertyName("description")] public string? Description { get; set; } = null!;
}