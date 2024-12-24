using System.Text.Json.Serialization;

namespace PostKit.Postmark.Email;

internal sealed class EmailRequest
{
    [JsonPropertyName("From")]
    public required string From { get; init; }

    [JsonPropertyName("ReplyTo")]
    public string? ReplyTo { get; init; }

    [JsonPropertyName("To")]
    public string? To { get; init; }

    [JsonPropertyName("Cc")]
    public string? Cc { get; init; }

    [JsonPropertyName("Bcc")]
    public string? Bcc { get; init; }

    [JsonPropertyName("Subject")]
    public string? Subject { get; init; }

    [JsonPropertyName("HtmlBody")]
    public string? HtmlBody { get; init; }

    [JsonPropertyName("TextBody")]
    public string? TextBody { get; init; }

    [JsonPropertyName("Attachments")]
    public List<EmailAttachmentRequest>? Attachments { get; init; }

    [JsonPropertyName("Tag")]
    public string? Tag { get; init; }

    [JsonPropertyName("Headers")]
    public IReadOnlyList<KeyValuePair<string, string>>? Headers { get; init; }

    [JsonPropertyName("Metadata")]
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }

    [JsonPropertyName("TrackOpens")]
    public bool? TrackOpens { get; init; }

    [JsonPropertyName("TrackLinks")]
    public string? TrackLinks { get; init; }

    [JsonPropertyName("MessageStream")]
    public string? MessageStream { get; init; }
}
