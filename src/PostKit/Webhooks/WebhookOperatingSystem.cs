using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents the operating system identified for an open or click webhook event.</summary>
public sealed record WebhookOperatingSystem
{
    /// <summary>Gets the identified operating system name when available.</summary>
    [JsonPropertyName("Name")]
    public string? Name { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets the identified operating system company when available.</summary>
    [JsonPropertyName("Company")]
    public string? Company { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets the identified operating system family when available.</summary>
    [JsonPropertyName("Family")]
    public string? Family { [UsedImplicitly] get; [UsedImplicitly] init; }
}
