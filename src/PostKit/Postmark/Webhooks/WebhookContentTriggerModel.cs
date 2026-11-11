using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Webhooks;

internal sealed class WebhookContentTriggerModel
{
    [JsonPropertyName("Enabled")]
    public bool? Enabled { get; [UsedImplicitly] init; }

    [JsonPropertyName("IncludeContent")]
    public bool? IncludeContent { get; [UsedImplicitly] init; }

    [JsonPropertyName("Status")]
    public string? Status { get; [UsedImplicitly] init; }
}