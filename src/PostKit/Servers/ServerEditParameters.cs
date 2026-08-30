using JetBrains.Annotations;
using PostKit.Common;

namespace PostKit.Servers;

/// <summary>Represents parameters for editing a Postmark server.</summary>
public sealed record ServerEditParameters
{
    /// <summary>Gets the server name.</summary>
    public string? Name { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional server color.</summary>
    public ServerColor? Color { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets whether the SMTP API should be active.</summary>
    public bool? SmtpApiActivated { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets whether raw email processing should be enabled.</summary>
    public bool? RawEmailEnabled { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets the optional inbound webhook URL.</summary>
    public string? InboundHookUrl { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets the optional bounce webhook URL.</summary>
    public string? BounceHookUrl { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets the optional open webhook URL.</summary>
    public string? OpenHookUrl { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets the optional delivery webhook URL.</summary>
    public string? DeliveryHookUrl { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets whether Postmark should call the open webhook only for the first open.</summary>
    public bool? PostFirstOpenOnly { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets the optional inbound domain.</summary>
    public string? InboundDomain { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets the inbound spam threshold.</summary>
    public int? InboundSpamThreshold { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets whether open tracking should be enabled.</summary>
    public bool? TrackOpens { [UsedImplicitly] get; init; }

    /// <summary>Gets the link tracking mode.</summary>
    public LinkTracking? TrackLinks { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets whether bounce content should be included in webhook payloads.</summary>
    public bool? IncludeBounceContentInHook { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets the optional click webhook URL.</summary>
    public string? ClickHookUrl { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets whether SMTP API error webhooks should be enabled.</summary>
    public bool? EnableSmtpApiErrorHooks { [UsedImplicitly] get; [UsedImplicitly] init; }
}
