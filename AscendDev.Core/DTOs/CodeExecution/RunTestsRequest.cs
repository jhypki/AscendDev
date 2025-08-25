using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AscendDev.Core.DTOs.CodeExecution;

public class RunTestsRequest
{
    [Required(ErrorMessage = "Lesson ID is required")]
    [JsonPropertyName("lessonId")]
    public string LessonId { get; set; } = null!;

    /// <summary>
    /// Direct code submission (for lessons without templates)
    /// </summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>
    /// Template-based submissions - only editable region content
    /// Key: region ID, Value: user-provided content for that region
    /// </summary>
    [JsonPropertyName("editableRegions")]
    public Dictionary<string, string>? EditableRegions { get; set; }

    /// <summary>
    /// Indicates whether this is a template-based submission or direct code submission
    /// </summary>
    [JsonIgnore]
    public bool IsTemplateBasedSubmission => EditableRegions != null && EditableRegions.Any();

    /// <summary>
    /// Validates that either Code or EditableRegions is provided
    /// </summary>
    public bool IsValid => !string.IsNullOrEmpty(Code) || (EditableRegions != null && EditableRegions.Any());
}