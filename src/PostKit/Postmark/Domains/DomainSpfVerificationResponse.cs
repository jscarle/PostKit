using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Domains;

internal sealed class DomainSpfVerificationResponse
{
    [JsonPropertyName("SPFHost")]
    public string? SpfHost { get; [UsedImplicitly] init; }

    [JsonPropertyName("SPFVerified")]
    public bool? SpfVerified { get; [UsedImplicitly] init; }

    [JsonPropertyName("SPFTextValue")]
    public string? SpfTextValue { get; [UsedImplicitly] init; }
}
