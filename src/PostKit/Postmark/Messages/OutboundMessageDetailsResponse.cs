using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Messages;

internal sealed class OutboundMessageDetailsResponse : OutboundMessageResponse
{
    [JsonPropertyName("TextBody")]
    public string? TextBody { get; [UsedImplicitly] init; }

    [JsonPropertyName("HtmlBody")]
    public string? HtmlBody { get; [UsedImplicitly] init; }

    [JsonPropertyName("Body")]
    public string? Body { get; [UsedImplicitly] init; }

    [JsonPropertyName("MessageEvents")]
    // ReSharper disable once CollectionNeverUpdated.Global
    public List<OutboundMessageEventResponse?>? MessageEvents { get; [UsedImplicitly] init; }
}