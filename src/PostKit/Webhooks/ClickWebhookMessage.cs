using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents an outbound-message link click reported by Postmark.</summary>
public sealed record ClickWebhookMessage : OutboundWebhookMessage
{
    /// <summary>Gets whether the link appeared in the HTML or text message body.</summary>
    [JsonPropertyName("ClickLocation")]
    public required string ClickLocation { [UsedImplicitly] get; init; }

    /// <summary>Gets the identified email client when available.</summary>
    [JsonPropertyName("Client")]
    public WebhookClient? Client { [UsedImplicitly] get; init; }

    /// <summary>Gets the identified operating system when available.</summary>
    [JsonPropertyName("OS")]
    public WebhookOperatingSystem? Os { [UsedImplicitly] get; init; }

    /// <summary>Gets the identified platform when available.</summary>
    [JsonPropertyName("Platform")]
    public string? Platform { [UsedImplicitly] get; init; }

    /// <summary>Gets the user agent supplied by the clicking client.</summary>
    [JsonPropertyName("UserAgent")]
    public required string UserAgent { [UsedImplicitly] get; init; }

    /// <summary>Gets the original link clicked by the recipient.</summary>
    [JsonPropertyName("OriginalLink")]
    public required string OriginalLink { [UsedImplicitly] get; init; }

    /// <summary>Gets the identified geographic information when available.</summary>
    [JsonPropertyName("Geo")]
    public WebhookGeo? Geo { [UsedImplicitly] get; init; }

    /// <summary>Gets the Postmark message identifier.</summary>
    [JsonPropertyName("MessageID")]
    public required Guid MessageId { [UsedImplicitly] get; init; }

    /// <summary>Gets the custom metadata sent with the message.</summary>
    [JsonPropertyName("Metadata")]
    public required IReadOnlyDictionary<string, string?> Metadata { [UsedImplicitly] get; init; }

    /// <summary>Gets when Postmark received the click event.</summary>
    [JsonPropertyName("ReceivedAt")]
    public required DateTimeOffset ReceivedAt { [UsedImplicitly] get; init; }

    /// <summary>Gets the message tag when one was supplied.</summary>
    [JsonPropertyName("Tag")]
    public string? Tag { [UsedImplicitly] get; init; }

    /// <summary>Gets the recipient that clicked the link.</summary>
    [JsonPropertyName("Recipient")]
    public required string Recipient { [UsedImplicitly] get; init; }

    /// <summary>Gets the message stream ID.</summary>
    [JsonPropertyName("MessageStream")]
    public required string MessageStream { [UsedImplicitly] get; init; }
}
