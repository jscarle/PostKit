using JetBrains.Annotations;

namespace PostKit.Stats;

/// <summary>Represents open counts for one day.</summary>
public sealed record OutboundOpenStatsDay
{
    /// <summary>Gets the statistics date.</summary>
    public required DateOnly Date { [UsedImplicitly] get; init; }

    /// <summary>Gets the open count.</summary>
    public required int Opens { [UsedImplicitly] get; init; }

    /// <summary>Gets the unique open count.</summary>
    public required int Unique { [UsedImplicitly] get; init; }
}