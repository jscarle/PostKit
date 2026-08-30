using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Messages;

internal sealed class InboundSearchResponse
{
    [JsonPropertyName("TotalCount")]
    public int? TotalCount { get; [UsedImplicitly] init; }

    [JsonPropertyName("InboundMessages")]
    // ReSharper disable once CollectionNeverUpdated.Global
    public List<InboundMessageResponse?>? InboundMessages { get; [UsedImplicitly] init; }
}
