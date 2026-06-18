namespace PostKit.Messages;

/// <summary>Identifies an inbound message status filter.</summary>
public enum InboundMessageStatus
{
    /// <summary>The message was blocked.</summary>
    Blocked,

    /// <summary>The message was processed.</summary>
    Processed,

    /// <summary>The message is queued.</summary>
    Queued,

    /// <summary>The message failed processing.</summary>
    Failed,

    /// <summary>The message is scheduled for processing.</summary>
    Scheduled
}