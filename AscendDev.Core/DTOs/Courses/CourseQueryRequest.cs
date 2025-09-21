using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AscendDev.Core.DTOs.Courses;

public class CourseQueryRequest
{
    [JsonPropertyName("search")]
    public string? Search { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    [JsonPropertyName("page")]
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;

    [JsonPropertyName("pageSize")]
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 12;

    [JsonPropertyName("sortBy")]
    public string? SortBy { get; set; } = "createdAt";

    [JsonPropertyName("sortOrder")]
    public string? SortOrder { get; set; } = "desc";
}