namespace PostKit.MessageStreams;

/// <summary>Identifies how unsubscribes should be handled for a message stream.</summary>
public enum UnsubscribeHandlingType
{
    /// <summary>Unsubscribe handling is disabled.</summary>
    None,

    /// <summary>Postmark manages unsubscribe handling.</summary>
    Postmark,

    /// <summary>A custom unsubscribe workflow handles unsubscribes.</summary>
    Custom
}
