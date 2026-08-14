using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents a Postmark webhook event for an outbound message.</summary>
public abstract record OutboundWebhookMessage
{
    /// <summary>Gets the Postmark webhook event type.</summary>
    [JsonPropertyName("RecordType")]
    public required string RecordType { [UsedImplicitly] get; init; }
}
