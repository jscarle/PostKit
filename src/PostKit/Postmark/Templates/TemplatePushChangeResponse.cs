using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Templates;

internal sealed class TemplatePushChangeResponse
{
    [JsonPropertyName("Action")]
    public string? Action { get; [UsedImplicitly] init; }

    [JsonPropertyName("TemplateId")]
    public long? TemplateId { get; [UsedImplicitly] init; }

    [JsonPropertyName("Alias")]
    public string? Alias { get; [UsedImplicitly] init; }

    [JsonPropertyName("Name")]
    public string? Name { get; [UsedImplicitly] init; }

    [JsonPropertyName("TemplateType")]
    public string? TemplateType { get; [UsedImplicitly] init; }
}
