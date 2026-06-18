using JetBrains.Annotations;

namespace PostKit.MessageStreams;

/// <summary>Represents a list of Postmark message streams.</summary>
public sealed record MessageStreamList
{
    /// <summary>Gets the message streams returned by Postmark.</summary>
    public required IReadOnlyList<MessageStreamInfo> MessageStreams { [UsedImplicitly] get; init; }
}
