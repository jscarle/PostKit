using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Webhooks;

internal sealed class WebhookListResponse
{
    [JsonPropertyName("Webhooks")]
    // ReSharper disable once CollectionNeverUpdated.Global
    public List<WebhookResponse?>? Webhooks { get; [UsedImplicitly] init; }
}
