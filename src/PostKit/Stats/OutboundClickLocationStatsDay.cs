using JetBrains.Annotations;

namespace PostKit.Stats;

/// <summary>Represents click location counts for one day.</summary>
public sealed record OutboundClickLocationStatsDay
{
    /// <summary>Gets the statistics date.</summary>
    public required DateOnly Date { [UsedImplicitly] get; init; }

    /// <summary>Gets the HTML body click count.</summary>
    public required int Html { [UsedImplicitly] get; init; }

    /// <summary>Gets the text body click count.</summary>
    public required int Text { [UsedImplicitly] get; init; }
}