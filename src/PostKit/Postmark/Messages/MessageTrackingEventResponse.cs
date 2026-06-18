using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Messages;

internal class MessageTrackingEventResponse
{
    [JsonPropertyName("RecordType")]
    public string? RecordType { get; [UsedImplicitly] init; }

    [JsonPropertyName("Client")]
    public MessageTrackingClientResponse? Client { get; [UsedImplicitly] init; }

    [JsonPropertyName("OS")]
    public MessageTrackingOperatingSystemResponse? Os { get; [UsedImplicitly] init; }

    [JsonPropertyName("Platform")]
    public string? Platform { get; [UsedImplicitly] init; }

    [JsonPropertyName("UserAgent")]
    public string? UserAgent { get; [UsedImplicitly] init; }

    [JsonPropertyName("Geo")]
    public MessageTrackingGeoResponse? Geo { get; [UsedImplicitly] init; }

    [JsonPropertyName("MessageID")]
    public string? MessageId { get; [UsedImplicitly] init; }

    [JsonPropertyName("MessageStream")]
    public string? MessageStream { get; [UsedImplicitly] init; }

    [JsonPropertyName("ReceivedAt")]
    public DateTimeOffset? ReceivedAt { get; [UsedImplicitly] init; }

    [JsonPropertyName("Tag")]
    public string? Tag { get; [UsedImplicitly] init; }

    [JsonPropertyName("Recipient")]
    public string? Recipient { get; [UsedImplicitly] init; }
}
