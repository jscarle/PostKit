using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Webhooks;

internal sealed class WebhookResponse
{
    [JsonPropertyName("ID")]
    public long? Id { get; [UsedImplicitly] init; }

    [JsonPropertyName("Url")]
    public string? Url { get; [UsedImplicitly] init; }

    [JsonPropertyName("MessageStream")]
    public string? MessageStream { get; [UsedImplicitly] init; }

    [JsonPropertyName("HttpAuth")]
    public WebhookHttpAuthModel? HttpAuth { get; [UsedImplicitly] init; }

    [JsonPropertyName("HttpHeaders")]
    public List<WebhookHeaderModel?>? HttpHeaders { get; [UsedImplicitly] init; }

    [JsonPropertyName("Triggers")]
    public WebhookTriggersModel? Triggers { get; [UsedImplicitly] init; }
}