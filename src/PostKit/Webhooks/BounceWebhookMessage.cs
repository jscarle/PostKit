using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents an outbound-message bounce reported by Postmark.</summary>
public sealed record BounceWebhookMessage : OutboundWebhookMessage
{
    /// <summary>Gets the Postmark bounce identifier.</summary>
    [JsonPropertyName("ID")]
    public required long Id { [UsedImplicitly] get; init; }

    /// <summary>Gets the Postmark bounce classification.</summary>
    [JsonPropertyName("Type")]
    public required string Type { [UsedImplicitly] get; init; }

    /// <summary>Gets the numeric Postmark bounce classification.</summary>
    [JsonPropertyName("TypeCode")]
    public required int TypeCode { [UsedImplicitly] get; init; }

    /// <summary>Gets the human-readable bounce type name.</summary>
    [JsonPropertyName("Name")]
    public required string Name { [UsedImplicitly] get; init; }

    /// <summary>Gets the message tag when one was supplied.</summary>
    [JsonPropertyName("Tag")]
    public string? Tag { [UsedImplicitly] get; init; }

    /// <summary>Gets the Postmark message identifier.</summary>
    [JsonPropertyName("MessageID")]
    public required Guid MessageId { [UsedImplicitly] get; init; }

    /// <summary>Gets the custom metadata sent with the message.</summary>
    [JsonPropertyName("Metadata")]
    public required IReadOnlyDictionary<string, string?> Metadata { [UsedImplicitly] get; init; }

    /// <summary>Gets the Postmark server identifier.</summary>
    [JsonPropertyName("ServerID")]
    public required long ServerId { [UsedImplicitly] get; init; }

    /// <summary>Gets the high-level bounce description.</summary>
    [JsonPropertyName("Description")]
    public required string Description { [UsedImplicitly] get; init; }

    /// <summary>Gets the provider-specific bounce details.</summary>
    [JsonPropertyName("Details")]
    public required string Details { [UsedImplicitly] get; init; }

    /// <summary>Gets the recipient address that bounced.</summary>
    [JsonPropertyName("Email")]
    public required string Email { [UsedImplicitly] get; init; }

    /// <summary>Gets the original sender address when available.</summary>
    [JsonPropertyName("From")]
    public string? From { [UsedImplicitly] get; init; }

    /// <summary>Gets the bounce timestamp.</summary>
    [JsonPropertyName("BouncedAt")]
    public required DateTimeOffset BouncedAt { [UsedImplicitly] get; init; }

    /// <summary>Gets whether a raw bounce dump is available.</summary>
    [JsonPropertyName("DumpAvailable")]
    public required bool DumpAvailable { [UsedImplicitly] get; init; }

    /// <summary>Gets whether the recipient is inactive.</summary>
    [JsonPropertyName("Inactive")]
    public required bool Inactive { [UsedImplicitly] get; init; }

    /// <summary>Gets whether the recipient can be reactivated.</summary>
    [JsonPropertyName("CanActivate")]
    public required bool CanActivate { [UsedImplicitly] get; init; }

    /// <summary>Gets the original message subject.</summary>
    [JsonPropertyName("Subject")]
    public required string Subject { [UsedImplicitly] get; init; }

    /// <summary>Gets the full bounce content when the webhook includes message content.</summary>
    [JsonPropertyName("Content")]
    public string? Content { [UsedImplicitly] get; init; }

    /// <summary>Gets the message stream ID.</summary>
    [JsonPropertyName("MessageStream")]
    public required string MessageStream { [UsedImplicitly] get; init; }
}
