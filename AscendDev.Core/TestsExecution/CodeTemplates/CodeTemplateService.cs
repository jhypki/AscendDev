using System.Text;
using System.Text.RegularExpressions;
using AscendDev.Core.Interfaces.TestsExecution;
using AscendDev.Core.Models.TestsExecution.CodeTemplates;
using Microsoft.Extensions.Logging;

namespace AscendDev.Core.TestsExecution.CodeTemplates;

public class CodeTemplateService : ICodeTemplateService
{
    private readonly ILogger<CodeTemplateService> _logger;

    public CodeTemplateService(ILogger<CodeTemplateService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string MergeTemplate(CodeTemplate template, Dictionary<string, string> editableRegionContent)
    {
        if (template == null)
            throw new ArgumentNullException(nameof(template));

        if (editableRegionContent == null)
            editableRegionContent = new Dictionary<string, string>();

        var result = new StringBuilder();

        foreach (var region in template.OrderedRegions)
        {
            if (region.IsEditable)
            {
                // Use provided content or fall back to existing content or placeholder
                var content = editableRegionContent.GetValueOrDefault(region.Id, region.Content);
                if (string.IsNullOrEmpty(content) && !string.IsNullOrEmpty(region.Placeholder))
                {
                    content = region.Placeholder;
                }
                result.Append(content);
            }
            else
            {
                // Non-editable regions use their original content
                result.Append(region.Content);
            }
        }

        return result.ToString();
    }

    public TemplateValidationResult ValidateEditableRegions(CodeTemplate template, Dictionary<string, string> editableRegionContent)
    {
        var result = new TemplateValidationResult { IsValid = true };

        if (template == null)
        {
            result.IsValid = false;
            result.Errors.Add("Template cannot be null");
            return result;
        }

        if (editableRegionContent == null)
            editableRegionContent = new Dictionary<string, string>();

        var editableRegions = template.EditableRegions.ToList();

        // Check for missing required editable regions
        foreach (var region in editableRegions)
        {
            if (!editableRegionContent.ContainsKey(region.Id))
            {
                result.MissingRegions.Add(region.Id);
                result.Errors.Add($"Missing content for editable region '{region.Id}' ({region.DisplayName ?? region.Id})");
            }
            else if (string.IsNullOrWhiteSpace(editableRegionContent[region.Id]))
            {
                // Allow empty content but warn about it
                _logger.LogWarning("Editable region '{RegionId}' has empty content", region.Id);
            }
        }

        // Check for invalid region IDs in provided content
        var validRegionIds = editableRegions.Select(r => r.Id).ToHashSet();
        foreach (var providedRegionId in editableRegionContent.Keys)
        {
            if (!validRegionIds.Contains(providedRegionId))
            {
                result.InvalidRegions.Add(providedRegionId);
                result.Errors.Add($"Invalid region ID '{providedRegionId}' - not found in template");
            }
        }

        result.IsValid = result.Errors.Count == 0;
        return result;
    }

    public Dictionary<string, string> ExtractEditableRegions(CodeTemplate template, string completeCode)
    {
        var result = new Dictionary<string, string>();

        if (template == null || string.IsNullOrEmpty(completeCode))
            return result;

        try
        {
            // This is a complex operation that would require sophisticated parsing
            // For now, we'll implement a basic version that works with simple templates

            var currentPosition = 0;
            var codeLength = completeCode.Length;

            foreach (var region in template.OrderedRegions)
            {
                if (region.IsEditable)
                {
                    // For editable regions, we need to extract the content between non-editable parts
                    // This is a simplified implementation - in practice, you might need more sophisticated parsing

                    var nextNonEditableRegion = template.OrderedRegions
                        .Where(r => !r.IsEditable && r.Order > region.Order)
                        .OrderBy(r => r.Order)
                        .FirstOrDefault();

                    if (nextNonEditableRegion != null)
                    {
                        var endPosition = completeCode.IndexOf(nextNonEditableRegion.Content, currentPosition, StringComparison.Ordinal);
                        if (endPosition >= currentPosition)
                        {
                            var extractedContent = completeCode.Substring(currentPosition, endPosition - currentPosition);
                            result[region.Id] = extractedContent;
                            currentPosition = endPosition;
                        }
                    }
                    else
                    {
                        // Last editable region - take everything remaining
                        if (currentPosition < codeLength)
                        {
                            result[region.Id] = completeCode.Substring(currentPosition);
                        }
                    }
                }
                else
                {
                    // Skip over non-editable content
                    var regionStart = completeCode.IndexOf(region.Content, currentPosition, StringComparison.Ordinal);
                    if (regionStart >= 0)
                    {
                        currentPosition = regionStart + region.Content.Length;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting editable regions from code");
        }

        return result;
    }

    public CodeTemplate CreateFromLegacyTemplate(string legacyTemplate, string language, string editableMarker = "//TODO")
    {
        if (string.IsNullOrEmpty(legacyTemplate))
            throw new ArgumentException("Legacy template cannot be null or empty", nameof(legacyTemplate));

        var template = new CodeTemplate
        {
            Name = "Legacy Template",
            Language = language,
            Description = "Converted from legacy template format",
            Version = "1.0.0"
        };

        // Split the template by the editable marker
        var parts = legacyTemplate.Split(new[] { editableMarker }, StringSplitOptions.None);
        var regions = new List<CodeTemplateRegion>();
        var order = 0;

        for (int i = 0; i < parts.Length; i++)
        {
            if (i == 0 || i == parts.Length - 1)
            {
                // First and last parts are typically non-editable
                if (!string.IsNullOrEmpty(parts[i]))
                {
                    regions.Add(new CodeTemplateRegion
                    {
                        Id = $"non_editable_{order}",
                        Content = parts[i],
                        IsEditable = false,
                        Order = order++
                    });
                }
            }
            else
            {
                // Middle parts alternate between editable markers and content
                if (i % 2 == 1)
                {
                    // This is where an editable region should be
                    regions.Add(new CodeTemplateRegion
                    {
                        Id = $"editable_{order}",
                        Content = "",
                        IsEditable = true,
                        DisplayName = $"Editable Region {(order + 1) / 2}",
                        Placeholder = "// Your code here",
                        Order = order++
                    });
                }

                // Add the content after the marker as non-editable
                if (!string.IsNullOrEmpty(parts[i]))
                {
                    regions.Add(new CodeTemplateRegion
                    {
                        Id = $"non_editable_{order}",
                        Content = parts[i],
                        IsEditable = false,
                        Order = order++
                    });
                }
            }
        }

        template.Regions = regions;
        return template;
    }
}