using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Servers;

internal sealed class ServerDeletionResponse
{
    [JsonPropertyName("ErrorCode")]
    public int? ErrorCode { get; [UsedImplicitly] init; }

    [JsonPropertyName("Message")]
    public string? Message { get; [UsedImplicitly] init; }
}