using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.DataRemovals;

internal sealed class DataRemovalCreateRequest
{
    [JsonPropertyName("RequestedBy")]
    public required string RequestedBy { [UsedImplicitly] get; init; }

    [JsonPropertyName("RequestedFor")]
    public required string RequestedFor { [UsedImplicitly] get; init; }

    [JsonPropertyName("NotifyWhenCompleted")]
    public required bool NotifyWhenCompleted { [UsedImplicitly] get; init; }
}
