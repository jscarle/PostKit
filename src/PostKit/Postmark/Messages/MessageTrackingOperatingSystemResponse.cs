using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Messages;

internal sealed class MessageTrackingOperatingSystemResponse
{
    [JsonPropertyName("Name")]
    public string? Name { get; [UsedImplicitly] init; }

    [JsonPropertyName("Company")]
    public string? Company { get; [UsedImplicitly] init; }

    [JsonPropertyName("Family")]
    public string? Family { get; [UsedImplicitly] init; }
}