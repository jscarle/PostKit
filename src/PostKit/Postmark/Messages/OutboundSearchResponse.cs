using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Messages;

internal sealed class OutboundSearchResponse
{
    [JsonPropertyName("TotalCount")]
    public int? TotalCount { get; [UsedImplicitly] init; }

    [JsonPropertyName("Messages")]
    // ReSharper disable once CollectionNeverUpdated.Global
    public List<OutboundMessageResponse?>? Messages { get; [UsedImplicitly] init; }
}
