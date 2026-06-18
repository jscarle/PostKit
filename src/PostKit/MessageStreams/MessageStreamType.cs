namespace PostKit.MessageStreams;

/// <summary>Identifies the kind of Postmark message stream.</summary>
public enum MessageStreamType
{
    /// <summary>An inbound message stream.</summary>
    Inbound,

    /// <summary>A transactional message stream.</summary>
    Transactional,

    /// <summary>A broadcast message stream.</summary>
    Broadcasts
}
