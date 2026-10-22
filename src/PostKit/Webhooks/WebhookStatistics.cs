using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents delivery statistics for a webhook.</summary>
public sealed record WebhookStatistics
{
    internal WebhookStatistics(long webhookId, long serverId, string messageStreamId, string url, IReadOnlyDictionary<string, string> statuses, WebhookStatisticsTimeRange timeRange, WebhookStatisticsMetrics metrics, IReadOnlyDictionary<string, WebhookStatisticsMetrics> metricsByTrigger)
    {
        WebhookId = webhookId;
        ServerId = serverId;
        MessageStreamId = messageStreamId;
        Url = url;
        Statuses = statuses;
        TimeRange = timeRange;
        Metrics = metrics;
        MetricsByTrigger = metricsByTrigger;
    }

    /// <summary>Gets the webhook identifier.</summary>
    public long WebhookId { [UsedImplicitly] get; }

    /// <summary>Gets the server identifier.</summary>
    public long ServerId { [UsedImplicitly] get; }

    /// <summary>Gets the message stream identifier.</summary>
    public string MessageStreamId { [UsedImplicitly] get; }

    /// <summary>Gets the webhook URL.</summary>
    public string Url { [UsedImplicitly] get; }

    /// <summary>Gets the provider's verification status for each trigger.</summary>
    public IReadOnlyDictionary<string, string> Statuses { [UsedImplicitly] get; }

    /// <summary>Gets the statistics window.</summary>
    public WebhookStatisticsTimeRange TimeRange { [UsedImplicitly] get; }

    /// <summary>Gets the aggregate delivery metrics.</summary>
    public WebhookStatisticsMetrics Metrics { [UsedImplicitly] get; }

    /// <summary>Gets the delivery metrics for each trigger.</summary>
    public IReadOnlyDictionary<string, WebhookStatisticsMetrics> MetricsByTrigger { [UsedImplicitly] get; }
}
