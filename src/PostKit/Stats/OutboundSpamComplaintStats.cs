using JetBrains.Annotations;

namespace PostKit.Stats;

/// <summary>Represents spam-complaint outbound statistics.</summary>
public sealed record OutboundSpamComplaintStats
{
    /// <summary>Gets the total spam complaint count.</summary>
    public required int SpamComplaint { [UsedImplicitly] get; init; }

    /// <summary>Gets the daily spam complaint counts.</summary>
    public required IReadOnlyList<OutboundSpamComplaintStatsDay> Days { [UsedImplicitly] get; init; }
}
