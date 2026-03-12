using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Bounces;

internal sealed class GetBouncesResponse
{
    [JsonPropertyName("TotalCount")]
    public int? TotalCount { get; [UsedImplicitly] init; }

    [JsonPropertyName("Bounces")]
    public List<BounceResponse>? Bounces { get; [UsedImplicitly] init; }
}
