using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents a successful outbound-message delivery reported by Postmark.</summary>
public sealed record DeliveryWebhookMessage : OutboundWebhookMessage
{
    /// <summary>Gets the Postmark message identifier.</summary>
    [JsonPropertyName("MessageID")]
    public required Guid MessageId { [UsedImplicitly] get; init; }

    /// <summary>Gets the delivered recipient address.</summary>
    [JsonPropertyName("Recipient")]
    public required string Recipient { [UsedImplicitly] get; init; }

    /// <summary>Gets the delivery timestamp.</summary>
    [JsonPropertyName("DeliveredAt")]
    public required DateTimeOffset DeliveredAt { [UsedImplicitly] get; init; }

    /// <summary>Gets the response received from the destination email server.</summary>
    [JsonPropertyName("Details")]
    public required string Details { [UsedImplicitly] get; init; }

    /// <summary>Gets the message tag when one was supplied.</summary>
    [JsonPropertyName("Tag")]
    public string? Tag { [UsedImplicitly] get; init; }

    /// <summary>Gets the Postmark server identifier.</summary>
    [JsonPropertyName("ServerID")]
    public required long ServerId { [UsedImplicitly] get; init; }

    /// <summary>Gets the custom metadata sent with the message.</summary>
    [JsonPropertyName("Metadata")]
    public required IReadOnlyDictionary<string, string?> Metadata { [UsedImplicitly] get; init; }

    /// <summary>Gets the message stream ID.</summary>
    [JsonPropertyName("MessageStream")]
    public required string MessageStream { [UsedImplicitly] get; init; }
}
