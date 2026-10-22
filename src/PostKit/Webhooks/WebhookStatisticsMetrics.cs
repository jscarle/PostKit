using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents webhook delivery metrics.</summary>
public sealed record WebhookStatisticsMetrics
{
    internal WebhookStatisticsMetrics(long totalRequests, long successCount, long failureCount, long retryCount, double successRate, long slowCount, long verySlowCount, long? averageTerminalResponseTimeMs, long? averageRetryResponseTimeMs)
    {
        TotalRequests = totalRequests;
        SuccessCount = successCount;
        FailureCount = failureCount;
        RetryCount = retryCount;
        SuccessRate = successRate;
        SlowCount = slowCount;
        VerySlowCount = verySlowCount;
        AverageTerminalResponseTimeMs = averageTerminalResponseTimeMs;
        AverageRetryResponseTimeMs = averageRetryResponseTimeMs;
    }

    /// <summary>Gets the total requests attempted.</summary>
    public long TotalRequests { [UsedImplicitly] get; }

    /// <summary>Gets the successful requests.</summary>
    public long SuccessCount { [UsedImplicitly] get; }

    /// <summary>Gets the failed requests.</summary>
    public long FailureCount { [UsedImplicitly] get; }

    /// <summary>Gets the retried requests.</summary>
    public long RetryCount { [UsedImplicitly] get; }

    /// <summary>Gets the success percentage.</summary>
    public double SuccessRate { [UsedImplicitly] get; }

    /// <summary>Gets the slow requests.</summary>
    public long SlowCount { [UsedImplicitly] get; }

    /// <summary>Gets the very slow requests.</summary>
    public long VerySlowCount { [UsedImplicitly] get; }

    /// <summary>Gets the average terminal response time in milliseconds, when samples are available.</summary>
    public long? AverageTerminalResponseTimeMs { [UsedImplicitly] get; }

    /// <summary>Gets the average retry response time in milliseconds, when available.</summary>
    public long? AverageRetryResponseTimeMs { [UsedImplicitly] get; }
}
