using JetBrains.Annotations;

namespace PostKit.Stats;

/// <summary>Represents email platform counts for one day.</summary>
public sealed record OutboundEmailPlatformStatsDay
{
    /// <summary>Gets the statistics date.</summary>
    public required DateOnly Date { [UsedImplicitly] get; init; }

    /// <summary>Gets the desktop platform count.</summary>
    public required int Desktop { [UsedImplicitly] get; init; }

    /// <summary>Gets the mobile platform count.</summary>
    public required int Mobile { [UsedImplicitly] get; init; }

    /// <summary>Gets the unknown platform count.</summary>
    public required int Unknown { [UsedImplicitly] get; init; }

    /// <summary>Gets the webmail platform count.</summary>
    public required int WebMail { [UsedImplicitly] get; init; }
}
