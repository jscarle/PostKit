namespace PostKit.Stats;

/// <summary>Represents email-client outbound statistics.</summary>
public sealed record OutboundEmailClientStats : OutboundUsageStats
{
    internal OutboundEmailClientStats(IReadOnlyDictionary<string, int> totals, IReadOnlyList<OutboundUsageStatsDay> days) : base(totals, days)
    {
    }
}
