using JetBrains.Annotations;

namespace PostKit.Stats;

/// <summary>Represents an outbound statistics overview.</summary>
public sealed record OutboundOverviewStats
{
    /// <summary>Gets the total sent count.</summary>
    public required int Sent { [UsedImplicitly] get; init; }

    /// <summary>Gets the total bounced count.</summary>
    public required int Bounced { [UsedImplicitly] get; init; }

    /// <summary>Gets the total SMTP API error count.</summary>
    public required int SmtpApiErrors { [UsedImplicitly] get; init; }

    /// <summary>Gets the bounce rate.</summary>
    public required double BounceRate { [UsedImplicitly] get; init; }

    /// <summary>Gets the total spam complaint count.</summary>
    public required int SpamComplaints { [UsedImplicitly] get; init; }

    /// <summary>Gets the spam complaint rate.</summary>
    public required double SpamComplaintsRate { [UsedImplicitly] get; init; }

    /// <summary>Gets the total open count.</summary>
    public required int Opens { [UsedImplicitly] get; init; }

    /// <summary>Gets the unique open count.</summary>
    public required int UniqueOpens { [UsedImplicitly] get; init; }

    /// <summary>Gets the total click count.</summary>
    public required int TotalClicks { [UsedImplicitly] get; init; }

    /// <summary>Gets the unique clicked-link count.</summary>
    public required int UniqueLinksClicked { [UsedImplicitly] get; init; }

    /// <summary>Gets the total tracked-link sent count.</summary>
    public required int TotalTrackedLinksSent { [UsedImplicitly] get; init; }

    /// <summary>Gets the total tracked email count.</summary>
    public required int Tracked { [UsedImplicitly] get; init; }

    /// <summary>Gets the count of messages with link tracking.</summary>
    public required int WithLinkTracking { [UsedImplicitly] get; init; }

    /// <summary>Gets the count of messages with open tracking.</summary>
    public required int WithOpenTracking { [UsedImplicitly] get; init; }

    /// <summary>Gets the count of messages with client data recorded.</summary>
    public required int WithClientRecorded { [UsedImplicitly] get; init; }

    /// <summary>Gets the count of messages with platform data recorded.</summary>
    public required int WithPlatformRecorded { [UsedImplicitly] get; init; }
}