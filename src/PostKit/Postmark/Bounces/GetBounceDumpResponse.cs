using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Bounces;

internal sealed class GetBounceDumpResponse
{
    [JsonPropertyName("Body")]
    public string? Body { get; [UsedImplicitly] init; }
}