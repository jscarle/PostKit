using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Messages;

internal sealed class OutboundMessageDumpResponse
{
    [JsonPropertyName("Body")]
    public string? Body { get; [UsedImplicitly] init; }
}