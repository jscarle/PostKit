using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Webhooks;

internal sealed class WebhookVerificationTriggerResponse
{
    [JsonPropertyName("TriggerType")]
    public string? TriggerType { get; [UsedImplicitly] init; }

    [JsonPropertyName("Success")]
    public bool? Success { get; [UsedImplicitly] init; }

    [JsonPropertyName("StatusCode")]
    public int? StatusCode { get; [UsedImplicitly] init; }

    [JsonPropertyName("Message")]
    public string? Message { get; [UsedImplicitly] init; }
}
