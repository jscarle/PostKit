using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Messages;

internal sealed class InboundMessageAddressResponse
{
    [JsonPropertyName("Email")]
    public string? Email { get; [UsedImplicitly] init; }

    [JsonPropertyName("Name")]
    public string? Name { get; [UsedImplicitly] init; }
}