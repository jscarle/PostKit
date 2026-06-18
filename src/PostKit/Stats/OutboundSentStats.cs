using JetBrains.Annotations;

namespace PostKit.Stats;

/// <summary>Represents sent-count outbound statistics.</summary>
public sealed record OutboundSentStats
{
    /// <summary>Gets the total sent count.</summary>
    public required int Sent { [UsedImplicitly] get; init; }

    /// <summary>Gets the daily sent counts.</summary>
    public required IReadOnlyList<OutboundSentStatsDay> Days { [UsedImplicitly] get; init; }
}
