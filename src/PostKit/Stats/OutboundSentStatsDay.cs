using JetBrains.Annotations;

namespace PostKit.Stats;

/// <summary>Represents sent counts for one day.</summary>
public sealed record OutboundSentStatsDay
{
    /// <summary>Gets the statistics date.</summary>
    public required DateOnly Date { [UsedImplicitly] get; init; }

    /// <summary>Gets the sent count.</summary>
    public required int Sent { [UsedImplicitly] get; init; }
}