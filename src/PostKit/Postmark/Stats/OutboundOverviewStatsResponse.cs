using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Stats;

internal sealed class OutboundOverviewStatsResponse
{
    [JsonPropertyName("Sent")]
    public int? Sent { get; [UsedImplicitly] init; }

    [JsonPropertyName("Bounced")]
    public int? Bounced { get; [UsedImplicitly] init; }

    [JsonPropertyName("SMTPApiErrors")]
    public int? SmtpApiErrors { get; [UsedImplicitly] init; }

    [JsonPropertyName("BounceRate")]
    public double? BounceRate { get; [UsedImplicitly] init; }

    [JsonPropertyName("SpamComplaints")]
    public int? SpamComplaints { get; [UsedImplicitly] init; }

    [JsonPropertyName("SpamComplaintsRate")]
    public double? SpamComplaintsRate { get; [UsedImplicitly] init; }

    [JsonPropertyName("Opens")]
    public int? Opens { get; [UsedImplicitly] init; }

    [JsonPropertyName("UniqueOpens")]
    public int? UniqueOpens { get; [UsedImplicitly] init; }

    [JsonPropertyName("TotalClicks")]
    public int? TotalClicks { get; [UsedImplicitly] init; }

    [JsonPropertyName("UniqueLinksClicked")]
    public int? UniqueLinksClicked { get; [UsedImplicitly] init; }

    [JsonPropertyName("TotalTrackedLinksSent")]
    public int? TotalTrackedLinksSent { get; [UsedImplicitly] init; }

    [JsonPropertyName("Tracked")]
    public int? Tracked { get; [UsedImplicitly] init; }

    [JsonPropertyName("WithLinkTracking")]
    public int? WithLinkTracking { get; [UsedImplicitly] init; }

    [JsonPropertyName("WithOpenTracking")]
    public int? WithOpenTracking { get; [UsedImplicitly] init; }

    [JsonPropertyName("WithClientRecorded")]
    public int? WithClientRecorded { get; [UsedImplicitly] init; }

    [JsonPropertyName("WithPlatformRecorded")]
    public int? WithPlatformRecorded { get; [UsedImplicitly] init; }
}
