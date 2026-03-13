using JetBrains.Annotations;

namespace PostKit.Bounces;

/// <summary>Represents bounce metadata returned from Postmark bounce endpoints.</summary>
public record Bounce
{
    /// <summary>Gets the record type returned by Postmark.</summary>
    public string RecordType { [UsedImplicitly] get; }

    /// <summary>Gets the bounce identifier.</summary>
    public long Id { [UsedImplicitly] get; }

    /// <summary>Gets the bounce classification.</summary>
    public BounceType Type { [UsedImplicitly] get; }

    /// <summary>Gets the numeric bounce type code returned by Postmark.</summary>
    public int TypeCode { [UsedImplicitly] get; }

    /// <summary>Gets the human-readable bounce type name.</summary>
    public string Name { [UsedImplicitly] get; }

    /// <summary>Gets the message tag associated with the bounced email.</summary>
    public string Tag { [UsedImplicitly] get; }

    /// <summary>Gets the Postmark message identifier for the bounced email.</summary>
    public Guid MessageId { [UsedImplicitly] get; }

    /// <summary>Gets the server identifier that owns the bounce.</summary>
    public long ServerId { [UsedImplicitly] get; }

    /// <summary>Gets the message stream ID associated with the bounced email.</summary>
    public string MessageStream { [UsedImplicitly] get; }

    /// <summary>Gets Postmark's high-level description of the bounce.</summary>
    public string Description { [UsedImplicitly] get; }

    /// <summary>Gets the lower-level provider details for the bounce.</summary>
    public string Details { [UsedImplicitly] get; }

    /// <summary>Gets the recipient email address that bounced.</summary>
    public string Email { [UsedImplicitly] get; }

    /// <summary>Gets the sender email address of the original message, when Postmark includes it.</summary>
    public string? From { [UsedImplicitly] get; }

    /// <summary>Gets the timestamp when Postmark recorded the bounce.</summary>
    public DateTimeOffset BouncedAt { [UsedImplicitly] get; }

    /// <summary>Gets a value indicating whether raw bounce content is available.</summary>
    public bool DumpAvailable { [UsedImplicitly] get; }

    /// <summary>Gets a value indicating whether the bounced recipient is inactive.</summary>
    public bool Inactive { [UsedImplicitly] get; }

    /// <summary>Gets a value indicating whether the bounce can be reactivated.</summary>
    public bool CanActivate { [UsedImplicitly] get; }

    /// <summary>Gets the subject of the bounced email.</summary>
    public string Subject { [UsedImplicitly] get; }

    internal Bounce(
        string recordType,
        long id,
        BounceType type,
        int typeCode,
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
        string subject
    )
    {
        RecordType = recordType;
        Id = id;
        Type = type;
        TypeCode = typeCode;
        Name = name;
        Tag = tag;
        MessageId = messageId;
        ServerId = serverId;
        MessageStream = messageStream;
        Description = description;
        Details = details;
        Email = email;
        From = from;
        BouncedAt = bouncedAt;
        DumpAvailable = dumpAvailable;
        Inactive = inactive;
        CanActivate = canActivate;
        Subject = subject;
    }
}
