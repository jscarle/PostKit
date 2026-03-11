using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Bounces;

internal sealed class BounceActivationResponse
{
    [JsonPropertyName("Message")]
    public string? Message { get; [UsedImplicitly] init; }

    [JsonPropertyName("Bounce")]
    public BounceResponse? Bounce { get; [UsedImplicitly] init; }
}
