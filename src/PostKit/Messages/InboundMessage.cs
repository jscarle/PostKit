using JetBrains.Annotations;

namespace PostKit.Messages;

/// <summary>Represents a summary of an inbound message.</summary>
public record InboundMessage
{
    internal InboundMessage(string from, string? fromName, InboundMessageAddress? fromFull, string to, IReadOnlyList<InboundMessageAddress> toFull, IReadOnlyList<InboundMessageAddress> ccFull, string? cc, string? replyTo,
        string originalRecipient, string? subject, string? date, string? mailboxHash, string? tag, IReadOnlyList<InboundMessageAttachment> attachments, Guid messageId, InboundMessageStatus status)
    {
        From = from;
        FromName = fromName;
        FromFull = fromFull;
        To = to;
        ToFull = toFull;
        CcFull = ccFull;
        Cc = cc;
        ReplyTo = replyTo;
        OriginalRecipient = originalRecipient;
        Subject = subject;
        Date = date;
        MailboxHash = mailboxHash;
        Tag = tag;
        Attachments = attachments;
        MessageId = messageId;
        Status = status;
    }

    /// <summary>Gets the sender email address string.</summary>
    public string From { [UsedImplicitly] get; }

    /// <summary>Gets the sender name.</summary>
    public string? FromName { [UsedImplicitly] get; }

    /// <summary>Gets the parsed sender details.</summary>
    public InboundMessageAddress? FromFull { [UsedImplicitly] get; }

    /// <summary>Gets the recipient string.</summary>
    public string To { [UsedImplicitly] get; }

    /// <summary>Gets the full To recipients.</summary>
    public IReadOnlyList<InboundMessageAddress> ToFull { [UsedImplicitly] get; }

    /// <summary>Gets the full Cc recipients.</summary>
    public IReadOnlyList<InboundMessageAddress> CcFull { [UsedImplicitly] get; }

    /// <summary>Gets the Cc recipient string.</summary>
    public string? Cc { [UsedImplicitly] get; }

    /// <summary>Gets the reply-to string.</summary>
    public string? ReplyTo { [UsedImplicitly] get; }

    /// <summary>Gets the original inbound recipient.</summary>
    public string OriginalRecipient { [UsedImplicitly] get; }

    /// <summary>Gets the message subject.</summary>
    public string? Subject { [UsedImplicitly] get; }

    /// <summary>Gets the original message date string returned by Postmark.</summary>
    public string? Date { [UsedImplicitly] get; }

    /// <summary>Gets the mailbox hash.</summary>
    public string? MailboxHash { [UsedImplicitly] get; }

    /// <summary>Gets the tag.</summary>
    public string? Tag { [UsedImplicitly] get; }

    /// <summary>Gets the attachments.</summary>
    public IReadOnlyList<InboundMessageAttachment> Attachments { [UsedImplicitly] get; }

    /// <summary>Gets the message ID.</summary>
    public Guid MessageId { [UsedImplicitly] get; }

    /// <summary>Gets the inbound message status.</summary>
    public InboundMessageStatus Status { [UsedImplicitly] get; }
}