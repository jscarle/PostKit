using System.Text.Json.Serialization;

namespace PostKit.Postmark.Email;

internal sealed class EmailRequest
{
    [JsonPropertyName("From")]
    public string? From { get; init; }

    [JsonPropertyName("ReplyTo")]
    public string? ReplyTo { get; set; }

    [JsonPropertyName("To")]
    public string? To { get; init; }

    [JsonPropertyName("Cc")]
    public string? Cc { get; set; }

    [JsonPropertyName("Bcc")]
    public string? Bcc { get; set; }

    [JsonPropertyName("Subject")]
    public string? Subject { get; set; }

    [JsonPropertyName("HtmlBody")]
    public string? HtmlBody { get; set; }

    [JsonPropertyName("TextBody")]
    public string? TextBody { get; set; }

    [JsonPropertyName("Attachments")]
    public List<EmailAttachmentRequest>? Attachments { get; set; }

    [JsonPropertyName("Tag")]
    public string? Tag { get; set; }

    [JsonPropertyName("Headers")]
    public List<EmailHeaderRequest>? Headers { get; set; }

    [JsonPropertyName("Metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    [JsonPropertyName("TrackOpens")]
    public bool? TrackOpens { get; set; }

    [JsonPropertyName("TrackLinks")]
    public string? TrackLinks { get; set; }

    [JsonPropertyName("MessageStream")]
    public string? MessageStream { get; set; }
}
