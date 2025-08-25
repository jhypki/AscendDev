using AscendDev.Core.Models.TestsExecution.CodeTemplates;

namespace AscendDev.Core.Interfaces.TestsExecution;

public interface ICodeTemplateService
{
    /// <summary>
    /// Merges user-provided editable region content with a code template
    /// </summary>
    /// <param name="template">The code template containing editable and non-editable regions</param>
    /// <param name="editableRegionContent">Dictionary mapping region IDs to user-provided content</param>
    /// <returns>The complete code with user content merged into the template</returns>
    string MergeTemplate(CodeTemplate template, Dictionary<string, string> editableRegionContent);

    /// <summary>
    /// Validates that all required editable regions have content provided
    /// </summary>
    /// <param name="template">The code template to validate against</param>
    /// <param name="editableRegionContent">Dictionary mapping region IDs to user-provided content</param>
    /// <returns>Validation result with any missing or invalid regions</returns>
    TemplateValidationResult ValidateEditableRegions(CodeTemplate template, Dictionary<string, string> editableRegionContent);

    /// <summary>
    /// Extracts editable region content from a complete code string based on a template
    /// This is useful for reverse-engineering user content from submitted code
    /// </summary>
    /// <param name="template">The code template used as reference</param>
    /// <param name="completeCode">The complete code string to extract from</param>
    /// <returns>Dictionary mapping region IDs to extracted content</returns>
    Dictionary<string, string> ExtractEditableRegions(CodeTemplate template, string completeCode);

    /// <summary>
    /// Creates a simple code template from a legacy template string
    /// This helps with backward compatibility
    /// </summary>
    /// <param name="legacyTemplate">The legacy template string</param>
    /// <param name="language">Programming language</param>
    /// <param name="editableMarker">Marker used to identify editable areas (default: "//TODO")</param>
    /// <returns>A CodeTemplate with appropriate regions</returns>
    CodeTemplate CreateFromLegacyTemplate(string legacyTemplate, string language, string editableMarker = "//TODO");
}

/// <summary>
/// Result of template validation
/// </summary>
public class TemplateValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> MissingRegions { get; set; } = new();
    public List<string> InvalidRegions { get; set; } = new();
}