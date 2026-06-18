using JetBrains.Annotations;

namespace PostKit.Stats;

/// <summary>Represents filters for outbound statistics endpoints.</summary>
public sealed record OutboundStatsQuery
{
    /// <summary>Gets the optional tag filter.</summary>
    public string? Tag { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional starting date filter.</summary>
    public DateOnly? FromDate { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional ending date filter.</summary>
    public DateOnly? ToDate { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional message stream ID filter. When omitted, Postmark includes all streams for the server.</summary>
    public string? MessageStreamId { [UsedImplicitly] get; init; }
}
