using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Templates;

internal sealed class TemplateRequest
{
    [JsonPropertyName("Name")]
    public required string Name { [UsedImplicitly] get; init; }

    [JsonPropertyName("Alias")]
    public string? Alias { [UsedImplicitly] get; init; }

    [JsonPropertyName("Subject")]
    public string? Subject { [UsedImplicitly] get; init; }

    [JsonPropertyName("HtmlBody")]
    public string? HtmlBody { [UsedImplicitly] get; init; }

    [JsonPropertyName("TextBody")]
    public string? TextBody { [UsedImplicitly] get; init; }

    [JsonPropertyName("TemplateType")]
    public string? TemplateType { [UsedImplicitly] get; init; }

    [JsonPropertyName("LayoutTemplate")]
    public string? LayoutTemplate { [UsedImplicitly] get; init; }
}