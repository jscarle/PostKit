using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Templates;

internal sealed class TemplateResponse : TemplateSummaryResponse
{
    [JsonPropertyName("Subject")]
    public string? Subject { get; [UsedImplicitly] init; }

    [JsonPropertyName("HtmlBody")]
    public string? HtmlBody { get; [UsedImplicitly] init; }

    [JsonPropertyName("TextBody")]
    public string? TextBody { get; [UsedImplicitly] init; }

    [JsonPropertyName("AssociatedServerId")]
    public long? AssociatedServerId { get; [UsedImplicitly] init; }
}