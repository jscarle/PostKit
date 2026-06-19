using JetBrains.Annotations;

namespace PostKit.MessageStreams;

/// <summary>Represents a Postmark message stream.</summary>
public sealed record MessageStreamInfo
{
    internal MessageStreamInfo(string id, long serverId, string name, string? description, MessageStreamType messageStreamType, DateTimeOffset createdAt, DateTimeOffset? updatedAt, DateTimeOffset? archivedAt,
        DateTimeOffset? expectedPurgeDate, MessageStreamSubscriptionManagementConfiguration? subscriptionManagementConfiguration)
    {
        Id = id;
        ServerId = serverId;
        Name = name;
        Description = description;
        MessageStreamType = messageStreamType;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        ArchivedAt = archivedAt;
        ExpectedPurgeDate = expectedPurgeDate;
        SubscriptionManagementConfiguration = subscriptionManagementConfiguration;
    }

    /// <summary>Gets the message stream ID.</summary>
    public string Id { [UsedImplicitly] get; }

    /// <summary>Gets the owning server ID.</summary>
    public long ServerId { [UsedImplicitly] get; }

    /// <summary>Gets the message stream name.</summary>
    public string Name { [UsedImplicitly] get; }

    /// <summary>Gets the message stream description.</summary>
    public string? Description { [UsedImplicitly] get; }

    /// <summary>Gets the message stream type.</summary>
    public MessageStreamType MessageStreamType { [UsedImplicitly] get; }

    /// <summary>Gets when the message stream was created.</summary>
    public DateTimeOffset CreatedAt { [UsedImplicitly] get; }

    /// <summary>Gets when the message stream was last updated.</summary>
    public DateTimeOffset? UpdatedAt { [UsedImplicitly] get; }

    /// <summary>Gets when the message stream was archived, when archived.</summary>
    public DateTimeOffset? ArchivedAt { [UsedImplicitly] get; }

    /// <summary>Gets when Postmark expects to purge the archived stream, when archived.</summary>
    public DateTimeOffset? ExpectedPurgeDate { [UsedImplicitly] get; }

    /// <summary>Gets the subscription-management settings.</summary>
    public MessageStreamSubscriptionManagementConfiguration? SubscriptionManagementConfiguration { [UsedImplicitly] get; }
}