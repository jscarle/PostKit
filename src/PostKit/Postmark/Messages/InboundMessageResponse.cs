using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Messages;

internal class InboundMessageResponse
{
    [JsonPropertyName("From")]
    public string? From { get; [UsedImplicitly] init; }

    [JsonPropertyName("FromName")]
    public string? FromName { get; [UsedImplicitly] init; }

    [JsonPropertyName("FromFull")]
    public InboundMessageAddressResponse? FromFull { get; [UsedImplicitly] init; }

    [JsonPropertyName("To")]
    public string? To { get; [UsedImplicitly] init; }

    [JsonPropertyName("ToFull")]
    public List<InboundMessageAddressResponse?>? ToFull { get; [UsedImplicitly] init; }

    [JsonPropertyName("CcFull")]
    public List<InboundMessageAddressResponse?>? CcFull { get; [UsedImplicitly] init; }

    [JsonPropertyName("Cc")]
    public string? Cc { get; [UsedImplicitly] init; }

    [JsonPropertyName("ReplyTo")]
    public string? ReplyTo { get; [UsedImplicitly] init; }

    [JsonPropertyName("OriginalRecipient")]
    public string? OriginalRecipient { get; [UsedImplicitly] init; }

    [JsonPropertyName("Subject")]
    public string? Subject { get; [UsedImplicitly] init; }

    [JsonPropertyName("Date")]
    public string? Date { get; [UsedImplicitly] init; }

    [JsonPropertyName("MailboxHash")]
    public string? MailboxHash { get; [UsedImplicitly] init; }

    [JsonPropertyName("Tag")]
    public string? Tag { get; [UsedImplicitly] init; }

    [JsonPropertyName("Attachments")]
    public List<InboundMessageAttachmentResponse?>? Attachments { get; [UsedImplicitly] init; }

    [JsonPropertyName("MessageID")]
    public string? MessageId { get; [UsedImplicitly] init; }

    [JsonPropertyName("Status")]
    public string? Status { get; [UsedImplicitly] init; }
}