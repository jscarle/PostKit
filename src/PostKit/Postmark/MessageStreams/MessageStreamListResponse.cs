using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.MessageStreams;

internal sealed class MessageStreamListResponse
{
    [JsonPropertyName("MessageStreams")]
    public List<MessageStreamResponse?>? MessageStreams { get; [UsedImplicitly] init; }
}
