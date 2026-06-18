using JetBrains.Annotations;

namespace PostKit.Stats;

/// <summary>Represents bounce-count outbound statistics.</summary>
public sealed record OutboundBounceStats
{
    /// <summary>Gets the hard-bounce count.</summary>
    public required int HardBounce { [UsedImplicitly] get; init; }

    /// <summary>Gets the SMTP API error count.</summary>
    public required int SmtpApiError { [UsedImplicitly] get; init; }

    /// <summary>Gets the soft-bounce count.</summary>
    public required int SoftBounce { [UsedImplicitly] get; init; }

    /// <summary>Gets the transient bounce count.</summary>
    public required int Transient { [UsedImplicitly] get; init; }

    /// <summary>Gets the daily bounce counts.</summary>
    public required IReadOnlyList<OutboundBounceStatsDay> Days { [UsedImplicitly] get; init; }
}