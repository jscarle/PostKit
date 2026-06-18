using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Messages;

internal sealed class InboundMessageHeaderResponse
{
    [JsonPropertyName("Name")]
    public string? Name { get; [UsedImplicitly] init; }

    [JsonPropertyName("Value")]
    public string? Value { get; [UsedImplicitly] init; }
}
