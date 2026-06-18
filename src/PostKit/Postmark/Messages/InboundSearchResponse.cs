using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Messages;

internal sealed class InboundSearchResponse
{
    [JsonPropertyName("TotalCount")]
    public int? TotalCount { get; [UsedImplicitly] init; }

    [JsonPropertyName("InboundMessages")]
    public List<InboundMessageResponse?>? InboundMessages { get; [UsedImplicitly] init; }
}
