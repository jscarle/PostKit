namespace PostKit.MessageStreams;

/// <summary>Identifies message stream types that can be returned by a message stream list query.</summary>
public enum MessageStreamListType
{
    /// <summary>Includes all message streams.</summary>
    All,

    /// <summary>Includes only inbound message streams.</summary>
    Inbound,

    /// <summary>Includes only transactional message streams.</summary>
    Transactional,

    /// <summary>Includes only broadcast message streams.</summary>
    Broadcasts
}
