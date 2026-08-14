using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents a Postmark message-stream suppression change.</summary>
public sealed record SubscriptionChangeWebhookMessage : OutboundWebhookMessage
{
    /// <summary>Gets the related Postmark message identifier, or null for changes without a related message.</summary>
    [JsonPropertyName("MessageID")]
    public required Guid? MessageId { [UsedImplicitly] get; init; }

    /// <summary>Gets the Postmark server identifier.</summary>
    [JsonPropertyName("ServerID")]
    public required long ServerId { [UsedImplicitly] get; init; }

    /// <summary>Gets the message stream ID.</summary>
    [JsonPropertyName("MessageStream")]
    public required string MessageStream { [UsedImplicitly] get; init; }

    /// <summary>Gets when the suppression state changed.</summary>
    [JsonPropertyName("ChangedAt")]
    public required DateTimeOffset ChangedAt { [UsedImplicitly] get; init; }

    /// <summary>Gets the recipient whose suppression state changed.</summary>
    [JsonPropertyName("Recipient")]
    public required string Recipient { [UsedImplicitly] get; init; }

    /// <summary>Gets where the subscription change originated.</summary>
    [JsonPropertyName("Origin")]
    public required string Origin { [UsedImplicitly] get; init; }

    /// <summary>Gets whether sending is suppressed for the recipient.</summary>
    [JsonPropertyName("SuppressSending")]
    public required bool SuppressSending { [UsedImplicitly] get; init; }

    /// <summary>Gets the suppression reason, or null for a reactivation.</summary>
    [JsonPropertyName("SuppressionReason")]
    public required string? SuppressionReason { [UsedImplicitly] get; init; }

    /// <summary>Gets the related message tag, or null for a reactivation.</summary>
    [JsonPropertyName("Tag")]
    public required string? Tag { [UsedImplicitly] get; init; }

    /// <summary>Gets the custom metadata associated with the subscription change.</summary>
    [JsonPropertyName("Metadata")]
    public required IReadOnlyDictionary<string, string?> Metadata { [UsedImplicitly] get; init; }
}
