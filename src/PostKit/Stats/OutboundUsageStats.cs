using JetBrains.Annotations;

namespace PostKit.Stats;

/// <summary>Represents keyed outbound usage statistics where Postmark returns usage names as JSON property names.</summary>
public abstract record OutboundUsageStats
{
    private protected OutboundUsageStats(IReadOnlyDictionary<string, int> totals, IReadOnlyList<OutboundUsageStatsDay> days)
    {
        Totals = totals;
        Days = days;
    }

    /// <summary>Gets the total counts by usage name.</summary>
    public IReadOnlyDictionary<string, int> Totals { [UsedImplicitly] get; }

    /// <summary>Gets the daily counts by usage name.</summary>
    public IReadOnlyList<OutboundUsageStatsDay> Days { [UsedImplicitly] get; }
}