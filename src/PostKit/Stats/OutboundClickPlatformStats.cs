using JetBrains.Annotations;

namespace PostKit.Stats;

/// <summary>Represents click-platform outbound statistics.</summary>
public sealed record OutboundClickPlatformStats
{
    /// <summary>Gets the desktop platform count.</summary>
    public required int Desktop { [UsedImplicitly] get; init; }

    /// <summary>Gets the mobile platform count.</summary>
    public required int Mobile { [UsedImplicitly] get; init; }

    /// <summary>Gets the unknown platform count.</summary>
    public required int Unknown { [UsedImplicitly] get; init; }

    /// <summary>Gets the daily click platform counts.</summary>
    public required IReadOnlyList<OutboundClickPlatformStatsDay> Days { [UsedImplicitly] get; init; }
}
