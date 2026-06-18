using JetBrains.Annotations;

namespace PostKit.Messages;

/// <summary>Represents a page of message open events.</summary>
public sealed record MessageOpenPage
{
    /// <summary>Gets the total matching open count.</summary>
    public required int TotalCount { [UsedImplicitly] get; init; }

    /// <summary>Gets the open events returned for this page.</summary>
    public required IReadOnlyList<MessageOpen> Opens { [UsedImplicitly] get; init; }
}
