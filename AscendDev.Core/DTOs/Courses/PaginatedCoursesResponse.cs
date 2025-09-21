using System.Text.Json.Serialization;
using AscendDev.Core.Models.Courses;

namespace AscendDev.Core.DTOs.Courses;

public class PaginatedCoursesResponse
{
    [JsonPropertyName("courses")]
    public List<Course> Courses { get; set; } = new();

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("hasNextPage")]
    public bool HasNextPage { get; set; }

    [JsonPropertyName("hasPreviousPage")]
    public bool HasPreviousPage { get; set; }

    public PaginatedCoursesResponse()
    {
    }

    public PaginatedCoursesResponse(List<Course> courses, int totalCount, int page, int pageSize)
    {
        Courses = courses;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        HasNextPage = page < TotalPages;
        HasPreviousPage = page > 1;
    }
}