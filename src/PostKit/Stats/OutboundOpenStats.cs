using JetBrains.Annotations;

namespace PostKit.Stats;

/// <summary>Represents open-count outbound statistics.</summary>
public sealed record OutboundOpenStats
{
    /// <summary>Gets the open count.</summary>
    public required int Opens { [UsedImplicitly] get; init; }

    /// <summary>Gets the unique open count.</summary>
    public required int Unique { [UsedImplicitly] get; init; }

    /// <summary>Gets the daily open counts.</summary>
    public required IReadOnlyList<OutboundOpenStatsDay> Days { [UsedImplicitly] get; init; }
}