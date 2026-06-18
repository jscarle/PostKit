using JetBrains.Annotations;

namespace PostKit.Stats;

/// <summary>Represents spam complaint counts for one day.</summary>
public sealed record OutboundSpamComplaintStatsDay
{
    /// <summary>Gets the statistics date.</summary>
    public required DateOnly Date { [UsedImplicitly] get; init; }

    /// <summary>Gets the spam complaint count.</summary>
    public required int SpamComplaint { [UsedImplicitly] get; init; }
}