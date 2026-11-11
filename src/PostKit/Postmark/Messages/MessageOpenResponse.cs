using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Messages;

internal sealed class MessageOpenResponse : MessageTrackingEventResponse
{
    [JsonPropertyName("ReadSeconds")]
    public int? ReadSeconds { get; [UsedImplicitly] init; }
}