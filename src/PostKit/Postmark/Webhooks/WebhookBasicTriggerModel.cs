using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Webhooks;

internal sealed class WebhookBasicTriggerModel
{
    [JsonPropertyName("Enabled")]
    public bool? Enabled { get; [UsedImplicitly] init; }
}