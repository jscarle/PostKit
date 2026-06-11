using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Messages;

internal class OutboundMessageResponse
{
    [JsonPropertyName("Tag")]
    public string? Tag { get; [UsedImplicitly] init; }

    [JsonPropertyName("MessageID")]
    public string? MessageId { get; [UsedImplicitly] init; }

    [JsonPropertyName("MessageStream")]
    public string? MessageStream { get; [UsedImplicitly] init; }

    [JsonPropertyName("To")]
    // ReSharper disable once CollectionNeverUpdated.Global
    public List<OutboundMessageRecipientResponse?>? To { get; [UsedImplicitly] init; }

    [JsonPropertyName("Cc")]
    // ReSharper disable once CollectionNeverUpdated.Global
    public List<OutboundMessageRecipientResponse?>? Cc { get; [UsedImplicitly] init; }

    [JsonPropertyName("Bcc")]
    // ReSharper disable once CollectionNeverUpdated.Global
    public List<OutboundMessageRecipientResponse?>? Bcc { get; [UsedImplicitly] init; }

    [JsonPropertyName("Recipients")]
    // ReSharper disable once CollectionNeverUpdated.Global
    public List<string?>? Recipients { get; [UsedImplicitly] init; }

    [JsonPropertyName("ReceivedAt")]
    public DateTimeOffset? ReceivedAt { get; [UsedImplicitly] init; }

    [JsonPropertyName("From")]
    public string? From { get; [UsedImplicitly] init; }

    [JsonPropertyName("Subject")]
    public string? Subject { get; [UsedImplicitly] init; }

    [JsonPropertyName("Attachments")]
    // ReSharper disable once CollectionNeverUpdated.Global
    public List<JsonElement>? Attachments { get; [UsedImplicitly] init; }

    [JsonPropertyName("Status")]
    public string? Status { get; [UsedImplicitly] init; }

    [JsonPropertyName("TrackOpens")]
    public bool? TrackOpens { get; [UsedImplicitly] init; }

    [JsonPropertyName("TrackLinks")]
    public string? TrackLinks { get; [UsedImplicitly] init; }

    [JsonPropertyName("Metadata")]
    public Dictionary<string, string?>? Metadata { get; [UsedImplicitly] init; }

    [JsonPropertyName("Sandboxed")]
    public bool? Sandboxed { get; [UsedImplicitly] init; }
}
