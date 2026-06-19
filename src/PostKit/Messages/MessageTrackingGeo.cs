using JetBrains.Annotations;

namespace PostKit.Messages;

/// <summary>Represents geographic details for a tracked message event.</summary>
public sealed record MessageTrackingGeo
{
    internal MessageTrackingGeo(string? countryIsoCode, string? country, string? regionIsoCode, string? region, string? city, string? zip, string? coords, string? ip)
    {
        CountryIsoCode = countryIsoCode;
        Country = country;
        RegionIsoCode = regionIsoCode;
        Region = region;
        City = city;
        Zip = zip;
        Coords = coords;
        Ip = ip;
    }

    /// <summary>Gets the ISO country code.</summary>
    public string? CountryIsoCode { [UsedImplicitly] get; }

    /// <summary>Gets the country name.</summary>
    public string? Country { [UsedImplicitly] get; }

    /// <summary>Gets the ISO region code.</summary>
    public string? RegionIsoCode { [UsedImplicitly] get; }

    /// <summary>Gets the region name.</summary>
    public string? Region { [UsedImplicitly] get; }

    /// <summary>Gets the city name.</summary>
    public string? City { [UsedImplicitly] get; }

    /// <summary>Gets the postal or ZIP code.</summary>
    public string? Zip { [UsedImplicitly] get; }

    /// <summary>Gets the geographic coordinates.</summary>
    public string? Coords { [UsedImplicitly] get; }

    /// <summary>Gets the IP address.</summary>
    public string? Ip { [UsedImplicitly] get; }
}