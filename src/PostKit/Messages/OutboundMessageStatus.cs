namespace PostKit.Messages;

/// <summary>Represents the processing status of an outbound message returned by Postmark.</summary>
public enum OutboundMessageStatus
{
    /// <summary>The message is queued for processing.</summary>
    Queued,

    /// <summary>The message has been sent.</summary>
    Sent,

    /// <summary>The message has been processed. Postmark treats this the same as <see cref="Sent" /> for outbound message search filters.</summary>
    Processed
}