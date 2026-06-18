using JetBrains.Annotations;

namespace PostKit.DataRemovals;

/// <summary>Represents a Postmark data removal request.</summary>
public sealed record DataRemoval
{
    /// <summary>Gets the data removal request ID.</summary>
    public required long Id { [UsedImplicitly] get; init; }

    /// <summary>Gets the data removal request status.</summary>
    public required DataRemovalStatus Status { [UsedImplicitly] get; init; }
}
