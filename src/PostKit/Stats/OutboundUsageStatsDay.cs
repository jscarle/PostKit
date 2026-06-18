using JetBrains.Annotations;

namespace PostKit.Stats;

/// <summary>Represents keyed outbound usage statistics for one day.</summary>
public sealed record OutboundUsageStatsDay
{
    /// <summary>Gets the statistics date.</summary>
    public required DateOnly Date { [UsedImplicitly] get; init; }

    /// <summary>Gets the usage counts by name.</summary>
    public required IReadOnlyDictionary<string, int> Counts { [UsedImplicitly] get; init; }
}