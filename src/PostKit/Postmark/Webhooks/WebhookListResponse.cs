using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Webhooks;

internal sealed class WebhookListResponse
{
    [JsonPropertyName("Webhooks")]
    public List<WebhookResponse?>? Webhooks { get; [UsedImplicitly] init; }
}