using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents the email client identified for an open or click webhook event.</summary>
public sealed record WebhookClient
{
    /// <summary>Gets the identified client name when available.</summary>
    [JsonPropertyName("Name")]
    public string? Name { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets the identified client company when available.</summary>
    [JsonPropertyName("Company")]
    public string? Company { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets the identified client family when available.</summary>
    [JsonPropertyName("Family")]
    public string? Family { [UsedImplicitly] get; [UsedImplicitly] init; }
}
