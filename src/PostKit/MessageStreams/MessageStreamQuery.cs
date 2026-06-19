using JetBrains.Annotations;

namespace PostKit.MessageStreams;

/// <summary>Represents filters for listing Postmark message streams.</summary>
public sealed record MessageStreamQuery
{
    /// <summary>Gets the optional message stream type filter.</summary>
    public MessageStreamListType? MessageStreamType { [UsedImplicitly] get; init; }

    /// <summary>Gets whether archived streams should be included.</summary>
    public bool? IncludeArchivedStreams { [UsedImplicitly] get; init; }
}