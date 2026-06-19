using JetBrains.Annotations;

namespace PostKit.Stats;

/// <summary>Represents bounce counts for one day.</summary>
public sealed record OutboundBounceStatsDay
{
    /// <summary>Gets the statistics date.</summary>
    public required DateOnly Date { [UsedImplicitly] get; init; }

    /// <summary>Gets the hard-bounce count.</summary>
    public required int HardBounce { [UsedImplicitly] get; init; }

    /// <summary>Gets the SMTP API error count.</summary>
    public required int SmtpApiError { [UsedImplicitly] get; init; }

    /// <summary>Gets the soft-bounce count.</summary>
    public required int SoftBounce { [UsedImplicitly] get; init; }

    /// <summary>Gets the transient bounce count.</summary>
    public required int Transient { [UsedImplicitly] get; init; }
}