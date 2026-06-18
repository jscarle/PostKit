using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Webhooks;

internal sealed class WebhookRequest
{
    [JsonPropertyName("Url")]
    public string? Url { [UsedImplicitly] get; init; }

    [JsonPropertyName("MessageStream")]
    public string? MessageStream { [UsedImplicitly] get; init; }

    [JsonPropertyName("HttpAuth")]
    public WebhookHttpAuthModel? HttpAuth { [UsedImplicitly] get; init; }

    [JsonPropertyName("HttpHeaders")]
    public IReadOnlyList<WebhookHeaderModel>? HttpHeaders { [UsedImplicitly] get; init; }

    [JsonPropertyName("Triggers")]
    public WebhookTriggersModel? Triggers { [UsedImplicitly] get; init; }
}
