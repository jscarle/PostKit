using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Messages;

internal sealed class OutboundMessageEventResponse
{
    [JsonPropertyName("Recipient")]
    public string? Recipient { get; [UsedImplicitly] init; }

    [JsonPropertyName("Type")]
    public string? Type { get; [UsedImplicitly] init; }

    [JsonPropertyName("ReceivedAt")]
    public DateTimeOffset? ReceivedAt { get; [UsedImplicitly] init; }

    [JsonPropertyName("Details")]
    public Dictionary<string, JsonElement>? Details { get; [UsedImplicitly] init; }
}