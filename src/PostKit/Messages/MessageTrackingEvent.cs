using JetBrains.Annotations;

namespace PostKit.Messages;

/// <summary>Represents the common details of a tracked message event.</summary>
public abstract record MessageTrackingEvent
{
    private protected MessageTrackingEvent(string? recordType, MessageTrackingClient? client, MessageTrackingOperatingSystem? os, string? platform, string userAgent, MessageTrackingGeo? geo, Guid messageId, string messageStream,
        DateTimeOffset receivedAt, string? tag, string? recipient)
    {
        RecordType = recordType;
        Client = client;
        Os = os;
        Platform = platform;
        UserAgent = userAgent;
        Geo = geo;
        MessageId = messageId;
        MessageStream = messageStream;
        ReceivedAt = receivedAt;
        Tag = tag;
        Recipient = recipient;
    }

    /// <summary>Gets the record type returned by Postmark, when present.</summary>
    public string? RecordType { [UsedImplicitly] get; }

    /// <summary>Gets the client details, when Postmark could determine them.</summary>
    public MessageTrackingClient? Client { [UsedImplicitly] get; }

    /// <summary>Gets the operating system details, when Postmark could determine them.</summary>
    public MessageTrackingOperatingSystem? Os { [UsedImplicitly] get; }

    /// <summary>Gets the platform, when Postmark could determine it.</summary>
    public string? Platform { [UsedImplicitly] get; }

    /// <summary>Gets the user-agent string.</summary>
    public string UserAgent { [UsedImplicitly] get; }

    /// <summary>Gets the geographic details, when Postmark could determine them.</summary>
    public MessageTrackingGeo? Geo { [UsedImplicitly] get; }

    /// <summary>Gets the message ID.</summary>
    public Guid MessageId { [UsedImplicitly] get; }

    /// <summary>Gets the message stream ID.</summary>
    public string MessageStream { [UsedImplicitly] get; }

    /// <summary>Gets when the event was received by Postmark.</summary>
    public DateTimeOffset ReceivedAt { [UsedImplicitly] get; }

    /// <summary>Gets the message tag, when returned.</summary>
    public string? Tag { [UsedImplicitly] get; }

    /// <summary>Gets the recipient, when returned.</summary>
    public string? Recipient { [UsedImplicitly] get; }
}