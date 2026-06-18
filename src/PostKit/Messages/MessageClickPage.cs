using JetBrains.Annotations;

namespace PostKit.Messages;

/// <summary>Represents a page of message click events.</summary>
public sealed record MessageClickPage
{
    /// <summary>Gets the total matching click count.</summary>
    public required int TotalCount { [UsedImplicitly] get; init; }

    /// <summary>Gets the click events returned for this page.</summary>
    public required IReadOnlyList<MessageClick> Clicks { [UsedImplicitly] get; init; }
}
