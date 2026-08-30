using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Messages;

internal sealed class MessageOpenSearchResponse
{
    [JsonPropertyName("TotalCount")]
    public int? TotalCount { get; [UsedImplicitly] init; }

    [JsonPropertyName("Opens")]
    // ReSharper disable once CollectionNeverUpdated.Global
    public List<MessageOpenResponse?>? Opens { get; [UsedImplicitly] init; }
}
