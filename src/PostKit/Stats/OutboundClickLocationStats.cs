using JetBrains.Annotations;

namespace PostKit.Stats;

/// <summary>Represents click-location outbound statistics.</summary>
public sealed record OutboundClickLocationStats
{
    /// <summary>Gets the HTML body click count.</summary>
    public required int Html { [UsedImplicitly] get; init; }

    /// <summary>Gets the text body click count.</summary>
    public required int Text { [UsedImplicitly] get; init; }

    /// <summary>Gets the daily click location counts.</summary>
    public required IReadOnlyList<OutboundClickLocationStatsDay> Days { [UsedImplicitly] get; init; }
}
