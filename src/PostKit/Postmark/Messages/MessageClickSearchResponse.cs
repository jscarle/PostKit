using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Messages;

internal sealed class MessageClickSearchResponse
{
    [JsonPropertyName("TotalCount")]
    public int? TotalCount { get; [UsedImplicitly] init; }

    [JsonPropertyName("Clicks")]
    // ReSharper disable once CollectionNeverUpdated.Global
    public List<MessageClickResponse?>? Clicks { get; [UsedImplicitly] init; }
}
