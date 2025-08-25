using System.Text.Json.Serialization;

namespace AscendDev.Core.Models.TestsExecution.CodeTemplates;

public class CodeTemplateRegion
{
    /// <summary>
    /// Unique identifier for this region within the template
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    /// <summary>
    /// The content of this region
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Whether this region can be edited by the user
    /// </summary>
    [JsonPropertyName("isEditable")]
    public bool IsEditable { get; set; }

    /// <summary>
    /// Display name for this region (for UI purposes)
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Placeholder text to show when the region is empty and editable
    /// </summary>
    [JsonPropertyName("placeholder")]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Order of this region in the template (0-based)
    /// </summary>
    [JsonPropertyName("order")]
    public int Order { get; set; }
}