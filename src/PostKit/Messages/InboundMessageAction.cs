using JetBrains.Annotations;

namespace PostKit.Messages;

/// <summary>Represents the result of an inbound message action.</summary>
public sealed record InboundMessageAction
{
    /// <summary>Gets the result message returned by Postmark.</summary>
    public required string Message { [UsedImplicitly] get; init; }
}
