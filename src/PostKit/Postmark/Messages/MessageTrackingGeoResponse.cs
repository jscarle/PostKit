using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Messages;

internal sealed class MessageTrackingGeoResponse
{
    [JsonPropertyName("CountryISOCode")]
    public string? CountryIsoCode { get; [UsedImplicitly] init; }

    [JsonPropertyName("Country")]
    public string? Country { get; [UsedImplicitly] init; }

    [JsonPropertyName("RegionISOCode")]
    public string? RegionIsoCode { get; [UsedImplicitly] init; }

    [JsonPropertyName("Region")]
    public string? Region { get; [UsedImplicitly] init; }

    [JsonPropertyName("City")]
    public string? City { get; [UsedImplicitly] init; }

    [JsonPropertyName("Zip")]
    public string? Zip { get; [UsedImplicitly] init; }

    [JsonPropertyName("Coords")]
    public string? Coords { get; [UsedImplicitly] init; }

    [JsonPropertyName("IP")]
    public string? Ip { get; [UsedImplicitly] init; }
}