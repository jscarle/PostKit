using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Bounces;

internal sealed class BounceCountElement
{
    [JsonPropertyName("Name")]
    public string? Name { get; [UsedImplicitly] init; }

    [JsonPropertyName("Count")]
    public int? Count { get; [UsedImplicitly] init; }

    [JsonPropertyName("Type")]
    public string? Type { get; [UsedImplicitly] init; }
}