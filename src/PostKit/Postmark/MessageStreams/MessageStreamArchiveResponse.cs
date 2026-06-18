using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.MessageStreams;

internal sealed class MessageStreamArchiveResponse
{
    [JsonPropertyName("ID")]
    public string? Id { get; [UsedImplicitly] init; }

    [JsonPropertyName("ServerID")]
    public long? ServerId { get; [UsedImplicitly] init; }

    [JsonPropertyName("ExpectedPurgeDate")]
    public DateTimeOffset? ExpectedPurgeDate { get; [UsedImplicitly] init; }
}