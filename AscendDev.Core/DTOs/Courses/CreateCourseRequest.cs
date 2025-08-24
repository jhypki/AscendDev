using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AscendDev.Core.DTOs.Courses;

public class CreateCourseRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 3)]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug must contain only lowercase letters, numbers, and hyphens")]
    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    [StringLength(1000)]
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [Required]
    [StringLength(50)]
    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    [Url]
    [JsonPropertyName("featuredImage")]
    public string? FeaturedImage { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "draft";
}