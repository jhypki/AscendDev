using System.Text.Json.Serialization;

namespace AscendDev.Core.Models.TestsExecution.CodeTemplates;

public class CodeTemplate
{
    /// <summary>
    /// Unique identifier for this template
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Name of the template for identification
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Programming language this template is for
    /// </summary>
    [JsonPropertyName("language")]
    public string Language { get; set; } = null!;

    /// <summary>
    /// Description of what this template does
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// List of regions that make up this template, ordered by their Order property
    /// </summary>
    [JsonPropertyName("regions")]
    public List<CodeTemplateRegion> Regions { get; set; } = new();

    /// <summary>
    /// Version of the template for tracking changes
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// When this template was created
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this template was last updated
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets all editable regions in order
    /// </summary>
    public IEnumerable<CodeTemplateRegion> EditableRegions =>
        Regions.Where(r => r.IsEditable).OrderBy(r => r.Order);

    /// <summary>
    /// Gets all non-editable regions in order
    /// </summary>
    public IEnumerable<CodeTemplateRegion> NonEditableRegions =>
        Regions.Where(r => !r.IsEditable).OrderBy(r => r.Order);

    /// <summary>
    /// Gets all regions in order
    /// </summary>
    public IEnumerable<CodeTemplateRegion> OrderedRegions =>
        Regions.OrderBy(r => r.Order);
}