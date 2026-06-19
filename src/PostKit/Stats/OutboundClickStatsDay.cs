using JetBrains.Annotations;

namespace PostKit.Stats;

/// <summary>Represents click counts for one day.</summary>
public sealed record OutboundClickStatsDay
{
    /// <summary>Gets the statistics date.</summary>
    public required DateOnly Date { [UsedImplicitly] get; init; }

    /// <summary>Gets the click count.</summary>
    public required int Clicks { [UsedImplicitly] get; init; }

    /// <summary>Gets the unique click count.</summary>
    public required int Unique { [UsedImplicitly] get; init; }
}