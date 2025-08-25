using System.Text.Json.Serialization;

namespace AscendDev.Core.Models.TestsExecution.KeywordValidation;

public class KeywordRequirement
{
    [JsonPropertyName("keyword")]
    public string Keyword { get; set; } = null!;

    [JsonPropertyName("description")]
    public string Description { get; set; } = null!;

    [JsonPropertyName("required")]
    public bool Required { get; set; } = true;

    [JsonPropertyName("caseSensitive")]
    public bool CaseSensitive { get; set; } = false;

    [JsonPropertyName("allowPartialMatch")]
    public bool AllowPartialMatch { get; set; } = false;

    [JsonPropertyName("minOccurrences")]
    public int MinOccurrences { get; set; } = 1;

    [JsonPropertyName("maxOccurrences")]
    public int? MaxOccurrences { get; set; }
}