namespace PostKit.Messages;

/// <summary>Represents the type of event recorded for an outbound message.</summary>
public enum OutboundMessageEventType
{
    /// <summary>A recipient's subscription state changed.</summary>
    SubscriptionChanged,

    /// <summary>The message was delivered.</summary>
    Delivered,

    /// <summary>The message encountered a transient delivery issue.</summary>
    Transient,

    /// <summary>The message was opened.</summary>
    Opened,

    /// <summary>A tracked link was clicked.</summary>
    LinkClicked,

    /// <summary>The message bounced.</summary>
    Bounced,
}
