using JetBrains.Annotations;

namespace PostKit.Messages;

/// <summary>Represents an event recorded for an outbound message.</summary>
public sealed record OutboundMessageEvent
{
    /// <summary>Gets the recipient associated with the event.</summary>
    public string Recipient { [UsedImplicitly] get; }

    /// <summary>Gets the event type.</summary>
    public OutboundMessageEventType Type { [UsedImplicitly] get; }

    /// <summary>Gets the timestamp when Postmark recorded the event.</summary>
    public DateTimeOffset ReceivedAt { [UsedImplicitly] get; }

    /// <summary>Gets the event-specific details returned by Postmark.</summary>
    public OutboundMessageEventDetails Details { [UsedImplicitly] get; }

    internal OutboundMessageEvent(string recipient, OutboundMessageEventType type, DateTimeOffset receivedAt, OutboundMessageEventDetails details)
    {
        Recipient = recipient;
        Type = type;
        ReceivedAt = receivedAt;
        Details = details;
    }
}
