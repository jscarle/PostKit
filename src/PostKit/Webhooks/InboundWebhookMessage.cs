using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents an inbound email delivered by a Postmark webhook.</summary>
public sealed record InboundWebhookMessage
{
    /// <summary>Gets the sender name.</summary>
    [JsonPropertyName("FromName")]
    public required string FromName { [UsedImplicitly] get; init; }

    /// <summary>Gets the inbound message stream ID.</summary>
    [JsonPropertyName("MessageStream")]
    public required string MessageStream { [UsedImplicitly] get; init; }

    /// <summary>Gets the formatted sender string supplied by Postmark.</summary>
    [JsonPropertyName("From")]
    public required string From { [UsedImplicitly] get; init; }

    /// <summary>Gets the structured sender address.</summary>
    [JsonPropertyName("FromFull")]
    public required InboundWebhookAddress FromFull { [UsedImplicitly] get; init; }

    /// <summary>Gets the formatted To recipient string supplied by Postmark.</summary>
    [JsonPropertyName("To")]
    public required string To { [UsedImplicitly] get; init; }

    /// <summary>Gets the structured To recipients.</summary>
    [JsonPropertyName("ToFull")]
    public required IReadOnlyList<InboundWebhookAddress?> ToFull { [UsedImplicitly] get; init; }

    /// <summary>Gets the formatted Cc recipient string supplied by Postmark.</summary>
    [JsonPropertyName("Cc")]
    public required string Cc { [UsedImplicitly] get; init; }

    /// <summary>Gets the structured Cc recipients.</summary>
    [JsonPropertyName("CcFull")]
    public required IReadOnlyList<InboundWebhookAddress?> CcFull { [UsedImplicitly] get; init; }

    /// <summary>Gets the formatted Bcc recipient string supplied by Postmark.</summary>
    [JsonPropertyName("Bcc")]
    public required string Bcc { [UsedImplicitly] get; init; }

    /// <summary>Gets the structured Bcc recipients.</summary>
    [JsonPropertyName("BccFull")]
    public required IReadOnlyList<InboundWebhookAddress?> BccFull { [UsedImplicitly] get; init; }

    /// <summary>Gets the original recipient used to route the inbound email.</summary>
    [JsonPropertyName("OriginalRecipient")]
    public required string OriginalRecipient { [UsedImplicitly] get; init; }

    /// <summary>Gets the message subject.</summary>
    [JsonPropertyName("Subject")]
    public required string Subject { [UsedImplicitly] get; init; }

    /// <summary>Gets the Postmark message ID.</summary>
    [JsonPropertyName("MessageID")]
    public required Guid MessageId { [UsedImplicitly] get; init; }

    /// <summary>Gets the reply-to value.</summary>
    [JsonPropertyName("ReplyTo")]
    public required string ReplyTo { [UsedImplicitly] get; init; }

    /// <summary>Gets the plus-addressing mailbox hash.</summary>
    [JsonPropertyName("MailboxHash")]
    public required string MailboxHash { [UsedImplicitly] get; init; }

    /// <summary>Gets the original date string supplied by the sending mail server.</summary>
    [JsonPropertyName("Date")]
    public required string Date { [UsedImplicitly] get; init; }

    /// <summary>Gets the plain-text message body.</summary>
    [JsonPropertyName("TextBody")]
    public required string TextBody { [UsedImplicitly] get; init; }

    /// <summary>Gets the HTML message body.</summary>
    [JsonPropertyName("HtmlBody")]
    public required string HtmlBody { [UsedImplicitly] get; init; }

    /// <summary>Gets the reply text stripped from the quoted conversation when Postmark can identify it.</summary>
    [JsonPropertyName("StrippedTextReply")]
    public required string StrippedTextReply { [UsedImplicitly] get; init; }

    /// <summary>Gets the inbound message tag.</summary>
    [JsonPropertyName("Tag")]
    public required string Tag { [UsedImplicitly] get; init; }

    /// <summary>Gets the email headers.</summary>
    [JsonPropertyName("Headers")]
    public required IReadOnlyList<InboundWebhookHeader?> Headers { [UsedImplicitly] get; init; }

    /// <summary>Gets the attachments.</summary>
    [JsonPropertyName("Attachments")]
    public required IReadOnlyList<InboundWebhookAttachment?> Attachments { [UsedImplicitly] get; init; }

    /// <summary>Gets the raw email when raw email retention is enabled for the Postmark server.</summary>
    [JsonPropertyName("RawEmail")]
    public string? RawEmail { [UsedImplicitly] get; init; }
}
