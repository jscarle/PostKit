using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Webhooks;

internal sealed class WebhookStatisticsTimeRangeResponse
{
    [JsonPropertyName("StartTime")]
    public DateTimeOffset? StartTime { get; [UsedImplicitly] init; }

    [JsonPropertyName("EndTime")]
    public DateTimeOffset? EndTime { get; [UsedImplicitly] init; }

    [JsonPropertyName("Hours")]
    public int? Hours { get; [UsedImplicitly] init; }
}
