using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Templates;

internal class TemplateSummaryResponse
{
    [JsonPropertyName("TemplateId")]
    public long? TemplateId { get; [UsedImplicitly] init; }

    [JsonPropertyName("Name")]
    public string? Name { get; [UsedImplicitly] init; }

    [JsonPropertyName("Active")]
    public bool? Active { get; [UsedImplicitly] init; }

    [JsonPropertyName("Alias")]
    public string? Alias { get; [UsedImplicitly] init; }

    [JsonPropertyName("TemplateType")]
    public string? TemplateType { get; [UsedImplicitly] init; }

    [JsonPropertyName("LayoutTemplate")]
    public string? LayoutTemplate { get; [UsedImplicitly] init; }
}