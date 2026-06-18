using JetBrains.Annotations;

namespace PostKit.Stats;

/// <summary>Represents email-platform outbound statistics.</summary>
public sealed record OutboundEmailPlatformStats
{
    /// <summary>Gets the desktop platform count.</summary>
    public required int Desktop { [UsedImplicitly] get; init; }

    /// <summary>Gets the mobile platform count.</summary>
    public required int Mobile { [UsedImplicitly] get; init; }

    /// <summary>Gets the unknown platform count.</summary>
    public required int Unknown { [UsedImplicitly] get; init; }

    /// <summary>Gets the webmail platform count.</summary>
    public required int WebMail { [UsedImplicitly] get; init; }

    /// <summary>Gets the daily email platform counts.</summary>
    public required IReadOnlyList<OutboundEmailPlatformStatsDay> Days { [UsedImplicitly] get; init; }
}
