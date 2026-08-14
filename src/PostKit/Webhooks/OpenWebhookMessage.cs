using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents an outbound-message open reported by Postmark.</summary>
public sealed record OpenWebhookMessage : OutboundWebhookMessage
{
    /// <summary>Gets whether this was the first recorded open for the recipient.</summary>
    [JsonPropertyName("FirstOpen")]
    public required bool FirstOpen { [UsedImplicitly] get; init; }

    /// <summary>Gets the identified email client when available.</summary>
    [JsonPropertyName("Client")]
    public WebhookClient? Client { [UsedImplicitly] get; init; }

    /// <summary>Gets the identified operating system when available.</summary>
    [JsonPropertyName("OS")]
    public WebhookOperatingSystem? Os { [UsedImplicitly] get; init; }

    /// <summary>Gets the identified platform when available.</summary>
    [JsonPropertyName("Platform")]
    public string? Platform { [UsedImplicitly] get; init; }

    /// <summary>Gets the user agent supplied by the opening client.</summary>
    [JsonPropertyName("UserAgent")]
    public required string UserAgent { [UsedImplicitly] get; init; }

    /// <summary>Gets the identified geographic information when available.</summary>
    [JsonPropertyName("Geo")]
    public WebhookGeo? Geo { [UsedImplicitly] get; init; }

    /// <summary>Gets the Postmark message identifier.</summary>
    [JsonPropertyName("MessageID")]
    public required Guid MessageId { [UsedImplicitly] get; init; }

    /// <summary>Gets the custom metadata sent with the message.</summary>
    [JsonPropertyName("Metadata")]
    public required IReadOnlyDictionary<string, string?> Metadata { [UsedImplicitly] get; init; }

    /// <summary>Gets when Postmark received the open event.</summary>
    [JsonPropertyName("ReceivedAt")]
    public required DateTimeOffset ReceivedAt { [UsedImplicitly] get; init; }

    /// <summary>Gets the message tag when one was supplied.</summary>
    [JsonPropertyName("Tag")]
    public string? Tag { [UsedImplicitly] get; init; }

    /// <summary>Gets the recipient that opened the message.</summary>
    [JsonPropertyName("Recipient")]
    public required string Recipient { [UsedImplicitly] get; init; }

    /// <summary>Gets the message stream ID.</summary>
    [JsonPropertyName("MessageStream")]
    public required string MessageStream { [UsedImplicitly] get; init; }
}
