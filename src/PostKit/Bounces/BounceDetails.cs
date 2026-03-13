using JetBrains.Annotations;

namespace PostKit.Bounces;

/// <summary>Represents a single bounce returned from the Postmark bounce details endpoint.</summary>
public sealed record BounceDetails : Bounce
{
    /// <summary>Gets the raw bounce dump content.</summary>
    public string Content { [UsedImplicitly] get; }

    internal BounceDetails(
        string recordType,
        long id,
        BounceType type,
        string name,
        string tag,
        Guid messageId,
        long serverId,
        string messageStream,
        string description,
        string details,
        string email,
        string? from,
        DateTimeOffset bouncedAt,
        bool dumpAvailable,
        bool inactive,
        bool canActivate,
        string subject,
        string content
    ) : base(
        recordType,
        id,
        type,
        name,
        tag,
        messageId,
        serverId,
        messageStream,
        description,
        details,
        email,
        from,
        bouncedAt,
        dumpAvailable,
        inactive,
        canActivate,
        subject
    )
    {
        Content = content;
    }
}
