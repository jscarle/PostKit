using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Messages;

internal sealed class InboundMessageDetailsResponse : InboundMessageResponse
{
    [JsonPropertyName("TextBody")]
    public string? TextBody { get; [UsedImplicitly] init; }

    [JsonPropertyName("HtmlBody")]
    public string? HtmlBody { get; [UsedImplicitly] init; }

    [JsonPropertyName("Headers")]
    public List<InboundMessageHeaderResponse?>? Headers { get; [UsedImplicitly] init; }

    [JsonPropertyName("BlockedReason")]
    public string? BlockedReason { get; [UsedImplicitly] init; }
}
