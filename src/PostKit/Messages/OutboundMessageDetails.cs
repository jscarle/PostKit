using JetBrains.Annotations;
using PostKit.Common;

namespace PostKit.Messages;

/// <summary>Represents a single outbound message returned from the Postmark outbound message details endpoint.</summary>
public sealed record OutboundMessageDetails : OutboundMessage
{
    /// <summary>Gets the text body of the message.</summary>
    public string TextBody { [UsedImplicitly] get; }

    /// <summary>Gets the HTML body of the message.</summary>
    public string HtmlBody { [UsedImplicitly] get; }

    /// <summary>Gets the raw source of the message.</summary>
    public string Body { [UsedImplicitly] get; }

    /// <summary>Gets the events Postmark has recorded for the message.</summary>
    public IReadOnlyCollection<OutboundMessageEvent> MessageEvents { [UsedImplicitly] get; }

    internal OutboundMessageDetails(
        string tag,
        Guid messageId,
        string messageStream,
        IReadOnlyCollection<OutboundMessageRecipient> to,
        IReadOnlyCollection<OutboundMessageRecipient> cc,
        IReadOnlyCollection<OutboundMessageRecipient> bcc,
        IReadOnlyCollection<string> recipients,
        DateTimeOffset receivedAt,
        string from,
        string subject,
        IReadOnlyCollection<OutboundMessageAttachment> attachments,
        OutboundMessageStatus status,
        bool trackOpens,
        LinkTracking trackLinks,
        IReadOnlyDictionary<string, string> metadata,
        bool sandboxed,
        string textBody,
        string htmlBody,
        string body,
        IReadOnlyCollection<OutboundMessageEvent> messageEvents
    )
        : base(tag, messageId, messageStream, to, cc, bcc, recipients, receivedAt, from, subject, attachments, status, trackOpens, trackLinks, metadata, sandboxed)
    {
        TextBody = textBody;
        HtmlBody = htmlBody;
        Body = body;
        MessageEvents = SnapshotCollection(messageEvents);
    }
}
