using JetBrains.Annotations;

namespace PostKit.Stats;

/// <summary>Represents tracked email counts for one day.</summary>
public sealed record OutboundTrackedEmailStatsDay
{
    /// <summary>Gets the statistics date.</summary>
    public required DateOnly Date { [UsedImplicitly] get; init; }

    /// <summary>Gets the tracked email count.</summary>
    public required int Tracked { [UsedImplicitly] get; init; }
}