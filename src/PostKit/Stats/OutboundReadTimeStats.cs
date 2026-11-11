namespace PostKit.Stats;

/// <summary>Represents outbound reading-time counts, grouped by Postmark's bucket names.</summary>
public sealed record OutboundReadTimeStats : OutboundUsageStats
{
    internal OutboundReadTimeStats(IReadOnlyDictionary<string, int> totals, IReadOnlyList<OutboundUsageStatsDay> days) : base(totals, days)
    {
    }
}
