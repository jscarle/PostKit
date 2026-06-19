using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Messages;

internal sealed class MessageClickResponse : MessageTrackingEventResponse
{
    [JsonPropertyName("ClickLocation")]
    public string? ClickLocation { get; [UsedImplicitly] init; }

    [JsonPropertyName("OriginalLink")]
    public string? OriginalLink { get; [UsedImplicitly] init; }
}