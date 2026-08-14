using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents geographic information identified for an open or click webhook event.</summary>
public sealed record WebhookGeo
{
    /// <summary>Gets the ISO country code when available.</summary>
    [JsonPropertyName("CountryISOCode")]
    public string? CountryIsoCode { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets the country name when available.</summary>
    [JsonPropertyName("Country")]
    public string? Country { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets the ISO region code when available.</summary>
    [JsonPropertyName("RegionISOCode")]
    public string? RegionIsoCode { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets the region name when available.</summary>
    [JsonPropertyName("Region")]
    public string? Region { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets the city name when available.</summary>
    [JsonPropertyName("City")]
    public string? City { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets the postal code when available.</summary>
    [JsonPropertyName("Zip")]
    public string? Zip { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets the geographic coordinates when available.</summary>
    [JsonPropertyName("Coords")]
    public string? Coords { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets the source IP address when available.</summary>
    [JsonPropertyName("IP")]
    public string? Ip { [UsedImplicitly] get; [UsedImplicitly] init; }
}
