using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents the window covered by webhook statistics.</summary>
public sealed record WebhookStatisticsTimeRange
{
    internal WebhookStatisticsTimeRange(DateTimeOffset startTime, DateTimeOffset endTime, int hours)
    {
        StartTime = startTime;
        EndTime = endTime;
        Hours = hours;
    }

    /// <summary>Gets the start of the window.</summary>
    public DateTimeOffset StartTime { [UsedImplicitly] get; }

    /// <summary>Gets the end of the window.</summary>
    public DateTimeOffset EndTime { [UsedImplicitly] get; }

    /// <summary>Gets the length of the window in hours.</summary>
    public int Hours { [UsedImplicitly] get; }
}
