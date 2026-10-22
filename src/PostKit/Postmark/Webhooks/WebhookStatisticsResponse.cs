using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Webhooks;

internal sealed class WebhookStatisticsResponse
{
    [JsonPropertyName("WebhookId")]
    public long? WebhookId { get; [UsedImplicitly] init; }

    [JsonPropertyName("ServerId")]
    public long? ServerId { get; [UsedImplicitly] init; }

    [JsonPropertyName("MessageStreamId")]
    public string? MessageStreamId { get; [UsedImplicitly] init; }

    [JsonPropertyName("Url")]
    public string? Url { get; [UsedImplicitly] init; }

    [JsonPropertyName("Statuses")]
    public IReadOnlyDictionary<string, string?>? Statuses { get; [UsedImplicitly] init; }

    [JsonPropertyName("TimeRange")]
    public WebhookStatisticsTimeRangeResponse? TimeRange { get; [UsedImplicitly] init; }

    [JsonPropertyName("Metrics")]
    public WebhookStatisticsMetricsResponse? Metrics { get; [UsedImplicitly] init; }

    [JsonPropertyName("MetricsByTrigger")]
    public IReadOnlyDictionary<string, WebhookStatisticsMetricsResponse?>? MetricsByTrigger { get; [UsedImplicitly] init; }
}
