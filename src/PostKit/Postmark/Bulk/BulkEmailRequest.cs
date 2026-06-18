using System.Text.Json.Serialization;
using JetBrains.Annotations;
using PostKit.Postmark.Email;

namespace PostKit.Postmark.Bulk;

internal sealed class BulkEmailRequest
{
    [JsonPropertyName("From")]
    public required string From { [UsedImplicitly] get; init; }

    [JsonPropertyName("ReplyTo")]
    public string? ReplyTo { [UsedImplicitly] get; init; }

    [JsonPropertyName("Subject")]
    public string? Subject { [UsedImplicitly] get; init; }

    [JsonPropertyName("HtmlBody")]
    public string? HtmlBody { [UsedImplicitly] get; init; }

    [JsonPropertyName("TextBody")]
    public string? TextBody { [UsedImplicitly] get; init; }

    [JsonPropertyName("TemplateId")]
    public int? TemplateId { [UsedImplicitly] get; init; }

    [JsonPropertyName("TemplateAlias")]
    public string? TemplateAlias { [UsedImplicitly] get; init; }

    [JsonPropertyName("InlineCss")]
    public bool? InlineCss { [UsedImplicitly] get; init; }

    [JsonPropertyName("Tag")]
    public string? Tag { [UsedImplicitly] get; init; }

    [JsonPropertyName("Metadata")]
    public IReadOnlyDictionary<string, string>? Metadata { [UsedImplicitly] get; init; }

    [JsonPropertyName("MessageStream")]
    public string? MessageStream { [UsedImplicitly] get; init; }

    [JsonPropertyName("TrackOpens")]
    public bool? TrackOpens { [UsedImplicitly] get; init; }

    [JsonPropertyName("TrackLinks")]
    public string? TrackLinks { [UsedImplicitly] get; init; }

    [JsonPropertyName("Attachments")]
    public IReadOnlyList<EmailRequestAttachment>? Attachments { [UsedImplicitly] get; init; }

    [JsonPropertyName("Headers")]
    public IReadOnlyList<EmailRequestHeader>? Headers { [UsedImplicitly] get; init; }

    [JsonPropertyName("Messages")]
    public required IReadOnlyList<BulkEmailMessageRequest> Messages { [UsedImplicitly] get; init; }
}
