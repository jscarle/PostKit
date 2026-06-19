using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.DataRemovals;

internal sealed class DataRemovalResponse
{
    [JsonPropertyName("ID")]
    public long? Id { get; [UsedImplicitly] init; }

    [JsonPropertyName("Status")]
    public string? Status { get; [UsedImplicitly] init; }
}