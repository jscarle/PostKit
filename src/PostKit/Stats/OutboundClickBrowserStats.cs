namespace PostKit.Stats;

/// <summary>Represents click-browser outbound statistics.</summary>
public sealed record OutboundClickBrowserStats : OutboundUsageStats
{
    internal OutboundClickBrowserStats(IReadOnlyDictionary<string, int> totals, IReadOnlyList<OutboundUsageStatsDay> days) : base(totals, days)
    {
    }
}