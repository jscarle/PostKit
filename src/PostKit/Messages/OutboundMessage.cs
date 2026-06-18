using System.Collections.ObjectModel;
using JetBrains.Annotations;
using PostKit.Common;

namespace PostKit.Messages;

/// <summary>Represents an outbound message summary returned from Postmark message endpoints.</summary>
public record OutboundMessage
{
    internal OutboundMessage(string tag, Guid messageId, string messageStream, IReadOnlyCollection<OutboundMessageRecipient> to, IReadOnlyCollection<OutboundMessageRecipient> cc, IReadOnlyCollection<OutboundMessageRecipient> bcc,
        IReadOnlyCollection<string> recipients, DateTimeOffset receivedAt, string from, string subject, IReadOnlyCollection<OutboundMessageAttachment> attachments, OutboundMessageStatus status, bool trackOpens, LinkTracking trackLinks,
        IReadOnlyDictionary<string, string> metadata, bool sandboxed)
    {
        Tag = tag;
        MessageId = messageId;
        MessageStream = messageStream;
        To = SnapshotCollection(to);
        Cc = SnapshotCollection(cc);
        Bcc = SnapshotCollection(bcc);
        Recipients = SnapshotCollection(recipients);
        ReceivedAt = receivedAt;
        From = from;
        Subject = subject;
        Attachments = SnapshotCollection(attachments);
        Status = status;
        TrackOpens = trackOpens;
        TrackLinks = trackLinks;
        Metadata = metadata switch
        {
            ReadOnlyDictionary<string, string> dictionary => dictionary,
            _ => new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(metadata, StringComparer.OrdinalIgnoreCase))
        };
        Sandboxed = sandboxed;
    }

    /// <summary>Gets the message tag.</summary>
    public string Tag { [UsedImplicitly] get; }

    /// <summary>Gets the Postmark message identifier.</summary>
    public Guid MessageId { [UsedImplicitly] get; }

    /// <summary>Gets the message stream ID associated with the message.</summary>
    public string MessageStream { [UsedImplicitly] get; }

    /// <summary>Gets the primary recipients.</summary>
    public IReadOnlyCollection<OutboundMessageRecipient> To { [UsedImplicitly] get; }

    /// <summary>Gets the carbon-copy recipients.</summary>
    public IReadOnlyCollection<OutboundMessageRecipient> Cc { [UsedImplicitly] get; }

    /// <summary>Gets the blind-carbon-copy recipients.</summary>
    public IReadOnlyCollection<OutboundMessageRecipient> Bcc { [UsedImplicitly] get; }

    /// <summary>Gets the recipient email addresses returned by Postmark.</summary>
    public IReadOnlyCollection<string> Recipients { [UsedImplicitly] get; }

    /// <summary>Gets the timestamp when Postmark received the message.</summary>
    public DateTimeOffset ReceivedAt { [UsedImplicitly] get; }

    /// <summary>Gets the sender address string returned by Postmark.</summary>
    public string From { [UsedImplicitly] get; }

    /// <summary>Gets the message subject.</summary>
    public string Subject { [UsedImplicitly] get; }

    /// <summary>Gets the attachments returned by Postmark.</summary>
    public IReadOnlyCollection<OutboundMessageAttachment> Attachments { [UsedImplicitly] get; }

    /// <summary>Gets the message status.</summary>
    public OutboundMessageStatus Status { [UsedImplicitly] get; }

    /// <summary>Gets a value indicating whether open tracking was enabled.</summary>
    public bool TrackOpens { [UsedImplicitly] get; }

    /// <summary>Gets the link tracking mode applied to the message.</summary>
    public LinkTracking TrackLinks { [UsedImplicitly] get; }

    /// <summary>Gets the metadata returned for the message.</summary>
    public IReadOnlyDictionary<string, string> Metadata { [UsedImplicitly] get; }

    /// <summary>Gets a value indicating whether the message was sandboxed.</summary>
    public bool Sandboxed { [UsedImplicitly] get; }

    private protected static ReadOnlyCollection<T> SnapshotCollection<T>(IReadOnlyCollection<T> values)
    {
        return values switch
        {
            ReadOnlyCollection<T> collection => collection,
            _ => new ReadOnlyCollection<T>(values.ToList())
        };
    }
}
