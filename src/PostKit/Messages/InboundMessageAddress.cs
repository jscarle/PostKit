using JetBrains.Annotations;

namespace PostKit.Messages;

/// <summary>Represents an inbound message address.</summary>
public sealed record InboundMessageAddress
{
    /// <summary>Gets the email address.</summary>
    public required string Email { [UsedImplicitly] get; init; }

    /// <summary>Gets the display name, when available.</summary>
    public required string? Name { [UsedImplicitly] get; init; }
}