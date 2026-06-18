using JetBrains.Annotations;

namespace PostKit.Messages;

/// <summary>Represents an inbound message header.</summary>
public sealed record InboundMessageHeader
{
    /// <summary>Gets the header name.</summary>
    public required string Name { [UsedImplicitly] get; init; }

    /// <summary>Gets the header value.</summary>
    public required string Value { [UsedImplicitly] get; init; }
}