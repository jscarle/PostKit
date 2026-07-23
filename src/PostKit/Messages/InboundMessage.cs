using JetBrains.Annotations;

namespace PostKit.Messages;

/// <summary>Represents a summary of an inbound message.</summary>
public record InboundMessage
{
    private readonly string? _legacyFrom;
    private readonly string? _legacyTo;
    private readonly string? _legacyCc;

    internal InboundMessage(InboundMessageAddress fromFull, IReadOnlyList<InboundMessageAddress> toFull, IReadOnlyList<InboundMessageAddress> ccFull, string? from, string? fromName, string? to, string? cc,
        string? replyTo, string originalRecipient, string? subject, string? date, string? mailboxHash, string? tag, IReadOnlyList<InboundMessageAttachment> attachments, Guid messageId, InboundMessageStatus status)
    {
        FromFull = fromFull;
        ToFull = toFull;
        CcFull = ccFull;
        _legacyFrom = from;
        FromName = fromName;
        _legacyTo = to;
        _legacyCc = cc;
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

    /// <summary>Gets the sender details.</summary>
    public InboundMessageAddress FromFull { [UsedImplicitly] get; }

    /// <summary>Gets the full To recipients.</summary>
    public IReadOnlyList<InboundMessageAddress> ToFull { [UsedImplicitly] get; }

    /// <summary>Gets the full Cc recipients.</summary>
    public IReadOnlyList<InboundMessageAddress> CcFull { [UsedImplicitly] get; }

    /// <summary>Gets the legacy sender contact string, when returned by Postmark.</summary>
    [Obsolete("Use FromFull instead.")]
    [UsedImplicitly]
    public string? From => _legacyFrom;

    /// <summary>Gets the sender name.</summary>
    public string? FromName { [UsedImplicitly] get; }

    /// <summary>Gets the legacy To contact string, when returned by Postmark.</summary>
    [Obsolete("Use ToFull instead.")]
    [UsedImplicitly]
    public string? To => _legacyTo;

    /// <summary>Gets the legacy Cc contact string, when returned by Postmark.</summary>
    [Obsolete("Use CcFull instead.")]
    [UsedImplicitly]
    public string? Cc => _legacyCc;

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
