using JetBrains.Annotations;

namespace PostKit.Stats;

/// <summary>Represents tracked-email outbound statistics.</summary>
public sealed record OutboundTrackedEmailStats
{
    /// <summary>Gets the total tracked email count.</summary>
    public required int Tracked { [UsedImplicitly] get; init; }

    /// <summary>Gets the daily tracked email counts.</summary>
    public required IReadOnlyList<OutboundTrackedEmailStatsDay> Days { [UsedImplicitly] get; init; }
}
