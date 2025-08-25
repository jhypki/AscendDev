using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AscendDev.Core.Models.Courses;
using AscendDev.Core.Models.TestsExecution.CodeTemplates;

namespace AscendDev.Core.DTOs.Lessons;

public class UpdateLessonRequest
{
    [StringLength(200, MinimumLength = 3)]
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [StringLength(100, MinimumLength = 3)]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug must contain only lowercase letters, numbers, and hyphens")]
    [JsonPropertyName("slug")]
    public string? Slug { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [StringLength(50)]
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>
    /// Code template with editable/non-editable regions
    /// </summary>
    [JsonPropertyName("codeTemplate")]
    public CodeTemplate? CodeTemplate { get; set; }

    [Range(1, int.MaxValue)]
    [JsonPropertyName("order")]
    public int? Order { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    [JsonPropertyName("testConfig")]
    public TestConfig? TestConfig { get; set; }

    [JsonPropertyName("additionalResources")]
    public List<AdditionalResource>? AdditionalResources { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}