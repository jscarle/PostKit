using JetBrains.Annotations;

namespace PostKit.MessageStreams;

/// <summary>Represents parameters for creating a Postmark message stream.</summary>
public sealed record MessageStreamCreateParameters
{
    /// <summary>Gets the message stream ID.</summary>
    public required string Id { [UsedImplicitly] get; init; }

    /// <summary>Gets the message stream name.</summary>
    public required string Name { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional message stream description.</summary>
    public string? Description { [UsedImplicitly] get; init; }

    /// <summary>Gets the message stream type. New streams can be transactional or broadcasts streams.</summary>
    public required MessageStreamType MessageStreamType { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional subscription-management settings.</summary>
    public MessageStreamSubscriptionManagementConfiguration? SubscriptionManagementConfiguration { [UsedImplicitly] get; init; }
}
