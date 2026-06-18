using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Messages;

internal sealed class InboundMessageActionResponse
{
    [JsonPropertyName("Message")]
    public string? Message { get; [UsedImplicitly] init; }
}
