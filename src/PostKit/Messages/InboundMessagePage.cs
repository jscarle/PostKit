using JetBrains.Annotations;

namespace PostKit.Messages;

/// <summary>Represents a page of inbound messages.</summary>
public sealed record InboundMessagePage
{
    /// <summary>Gets the total matching message count.</summary>
    public required int TotalCount { [UsedImplicitly] get; init; }

    /// <summary>Gets the inbound messages returned for this page.</summary>
    public required IReadOnlyList<InboundMessage> Messages { [UsedImplicitly] get; init; }
}