using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Webhooks;

internal sealed class WebhookOpenTriggerModel
{
    [JsonPropertyName("Enabled")]
    public bool? Enabled { get; [UsedImplicitly] init; }

    [JsonPropertyName("PostFirstOpenOnly")]
    public bool? PostFirstOpenOnly { get; [UsedImplicitly] init; }
}
