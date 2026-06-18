using JetBrains.Annotations;

namespace PostKit.MessageStreams;

/// <summary>Represents subscription-management settings for a message stream.</summary>
public sealed record MessageStreamSubscriptionManagementConfiguration
{
    /// <summary>Gets how unsubscribes should be handled.</summary>
    public UnsubscribeHandlingType? UnsubscribeHandlingType { [UsedImplicitly] get; init; }
}