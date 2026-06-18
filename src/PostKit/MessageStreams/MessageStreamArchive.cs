using JetBrains.Annotations;

namespace PostKit.MessageStreams;

/// <summary>Represents a successful message stream archive action.</summary>
public sealed record MessageStreamArchive
{
    /// <summary>Gets the archived message stream ID.</summary>
    public required string Id { [UsedImplicitly] get; init; }

    /// <summary>Gets the owning server ID.</summary>
    public required long ServerId { [UsedImplicitly] get; init; }

    /// <summary>Gets when Postmark expects to purge the archived stream.</summary>
    public required DateTimeOffset ExpectedPurgeDate { [UsedImplicitly] get; init; }
}
