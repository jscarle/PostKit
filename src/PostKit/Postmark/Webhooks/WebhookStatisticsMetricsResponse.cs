using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Webhooks;

internal sealed class WebhookStatisticsMetricsResponse
{
    [JsonPropertyName("TotalRequests")]
    public long? TotalRequests { get; [UsedImplicitly] init; }

    [JsonPropertyName("SuccessCount")]
    public long? SuccessCount { get; [UsedImplicitly] init; }

    [JsonPropertyName("FailureCount")]
    public long? FailureCount { get; [UsedImplicitly] init; }

    [JsonPropertyName("RetryCount")]
    public long? RetryCount { get; [UsedImplicitly] init; }

    [JsonPropertyName("SuccessRate")]
    public double? SuccessRate { get; [UsedImplicitly] init; }

    [JsonPropertyName("SlowCount")]
    public long? SlowCount { get; [UsedImplicitly] init; }

    [JsonPropertyName("VerySlowCount")]
    public long? VerySlowCount { get; [UsedImplicitly] init; }

    [JsonPropertyName("AverageTerminalResponseTimeMs")]
    public long? AverageTerminalResponseTimeMs { get; [UsedImplicitly] init; }

    [JsonPropertyName("AverageRetryResponseTimeMs")]
    public long? AverageRetryResponseTimeMs { get; [UsedImplicitly] init; }
}
