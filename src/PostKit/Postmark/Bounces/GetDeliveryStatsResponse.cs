using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Bounces;

internal sealed class GetDeliveryStatsResponse
{
    [JsonPropertyName("InactiveMails")]
    public int? InactiveMails { get; [UsedImplicitly] init; }

    [JsonPropertyName("Bounces")]
    // ReSharper disable once CollectionNeverUpdated.Global
    public List<BounceCountElement>? Bounces { get; [UsedImplicitly] init; }
}