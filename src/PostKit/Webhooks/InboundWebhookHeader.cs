using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents an email header in a Postmark inbound webhook.</summary>
public sealed record InboundWebhookHeader
{
    /// <summary>Gets the header name.</summary>
    [JsonPropertyName("Name")]
    public required string Name { [UsedImplicitly] get; init; }

    /// <summary>Gets the header value.</summary>
    [JsonPropertyName("Value")]
    public required string Value { [UsedImplicitly] get; init; }
}
