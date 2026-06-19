using JetBrains.Annotations;

namespace PostKit.Messages;

/// <summary>Represents inbound message details.</summary>
public sealed record InboundMessageDetails : InboundMessage
{
    internal InboundMessageDetails(InboundMessage message, string? textBody, string? htmlBody, IReadOnlyList<InboundMessageHeader> headers, string? blockedReason) : base(message.From, message.FromName, message.FromFull, message.To,
        message.ToFull, message.CcFull, message.Cc, message.ReplyTo, message.OriginalRecipient, message.Subject, message.Date, message.MailboxHash, message.Tag, message.Attachments, message.MessageId, message.Status)
    {
        TextBody = textBody;
        HtmlBody = htmlBody;
        Headers = headers;
        BlockedReason = blockedReason;
    }

    /// <summary>Gets the text body.</summary>
    public string? TextBody { [UsedImplicitly] get; }

    /// <summary>Gets the HTML body.</summary>
    public string? HtmlBody { [UsedImplicitly] get; }

    /// <summary>Gets the message headers.</summary>
    public IReadOnlyList<InboundMessageHeader> Headers { [UsedImplicitly] get; }

    /// <summary>Gets the blocked reason, when available.</summary>
    public string? BlockedReason { [UsedImplicitly] get; }
}