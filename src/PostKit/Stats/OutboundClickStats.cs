using JetBrains.Annotations;

namespace PostKit.Stats;

/// <summary>Represents click-count outbound statistics.</summary>
public sealed record OutboundClickStats
{
    /// <summary>Gets the click count.</summary>
    public required int Clicks { [UsedImplicitly] get; init; }

    /// <summary>Gets the unique click count.</summary>
    public required int Unique { [UsedImplicitly] get; init; }

    /// <summary>Gets the daily click counts.</summary>
    public required IReadOnlyList<OutboundClickStatsDay> Days { [UsedImplicitly] get; init; }
}
