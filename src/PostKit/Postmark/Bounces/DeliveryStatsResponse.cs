using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Bounces;

internal sealed class DeliveryStatsResponse
{
    [JsonPropertyName("InactiveMails")]
    public int? InactiveMails { get; [UsedImplicitly] init; }

    [JsonPropertyName("Bounces")]
    public List<BounceCountElement>? Bounces { get; [UsedImplicitly] init; }
}
