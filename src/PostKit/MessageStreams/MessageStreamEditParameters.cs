using JetBrains.Annotations;

namespace PostKit.MessageStreams;

/// <summary>Represents parameters for editing a Postmark message stream.</summary>
public sealed record MessageStreamEditParameters
{
    /// <summary>Gets the message stream name.</summary>
    public string? Name { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional message stream description.</summary>
    public string? Description { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional subscription-management settings.</summary>
    public MessageStreamSubscriptionManagementConfiguration? SubscriptionManagementConfiguration { [UsedImplicitly] get; init; }
}
