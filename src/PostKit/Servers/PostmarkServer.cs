using JetBrains.Annotations;
using PostKit.Common;

namespace PostKit.Servers;

/// <summary>Represents a Postmark server.</summary>
public sealed record PostmarkServer
{
    internal PostmarkServer(long id, string name, IReadOnlyList<string> apiTokens, ServerColor color, bool smtpApiActivated, bool rawEmailEnabled, ServerDeliveryType deliveryType, string? serverLink, string? inboundAddress,
        string? inboundHookUrl, string? bounceHookUrl, string? openHookUrl, string? deliveryHookUrl, bool postFirstOpenOnly, string? inboundDomain, string? inboundHash, int inboundSpamThreshold, bool trackOpens, LinkTracking trackLinks,
        bool includeBounceContentInHook, string? clickHookUrl, bool enableSmtpApiErrorHooks)
    {
        Id = id;
        Name = name;
        ApiTokens = apiTokens;
        Color = color;
        SmtpApiActivated = smtpApiActivated;
        RawEmailEnabled = rawEmailEnabled;
        DeliveryType = deliveryType;
        ServerLink = serverLink;
        InboundAddress = inboundAddress;
        InboundHookUrl = inboundHookUrl;
        BounceHookUrl = bounceHookUrl;
        OpenHookUrl = openHookUrl;
        DeliveryHookUrl = deliveryHookUrl;
        PostFirstOpenOnly = postFirstOpenOnly;
        InboundDomain = inboundDomain;
        InboundHash = inboundHash;
        InboundSpamThreshold = inboundSpamThreshold;
        TrackOpens = trackOpens;
        TrackLinks = trackLinks;
        IncludeBounceContentInHook = includeBounceContentInHook;
        ClickHookUrl = clickHookUrl;
        EnableSmtpApiErrorHooks = enableSmtpApiErrorHooks;
    }

    /// <summary>Gets the server ID.</summary>
    public long Id { [UsedImplicitly] get; }

    /// <summary>Gets the server name.</summary>
    public string Name { [UsedImplicitly] get; }

    /// <summary>Gets the server API tokens returned by Postmark.</summary>
    public IReadOnlyList<string> ApiTokens { [UsedImplicitly] get; }

    /// <summary>Gets the display color Postmark applies to the server.</summary>
    public ServerColor Color { [UsedImplicitly] get; }

    /// <summary>Gets whether the SMTP API is active.</summary>
    public bool SmtpApiActivated { [UsedImplicitly] get; }

    /// <summary>Gets whether raw email processing is enabled.</summary>
    public bool RawEmailEnabled { [UsedImplicitly] get; }

    /// <summary>Gets whether the server sends live or sandbox email.</summary>
    public ServerDeliveryType DeliveryType { [UsedImplicitly] get; }

    /// <summary>Gets the Postmark web app link for the server.</summary>
    public string? ServerLink { [UsedImplicitly] get; }

    /// <summary>Gets the generated inbound email address.</summary>
    public string? InboundAddress { [UsedImplicitly] get; }

    /// <summary>Gets the inbound webhook URL.</summary>
    public string? InboundHookUrl { [UsedImplicitly] get; }

    /// <summary>Gets the bounce webhook URL.</summary>
    public string? BounceHookUrl { [UsedImplicitly] get; }

    /// <summary>Gets the open webhook URL.</summary>
    public string? OpenHookUrl { [UsedImplicitly] get; }

    /// <summary>Gets the delivery webhook URL.</summary>
    public string? DeliveryHookUrl { [UsedImplicitly] get; }

    /// <summary>Gets whether Postmark calls the open webhook only for the first open.</summary>
    public bool PostFirstOpenOnly { [UsedImplicitly] get; }

    /// <summary>Gets the inbound domain.</summary>
    public string? InboundDomain { [UsedImplicitly] get; }

    /// <summary>Gets the inbound hash.</summary>
    public string? InboundHash { [UsedImplicitly] get; }

    /// <summary>Gets the inbound spam threshold.</summary>
    public int InboundSpamThreshold { [UsedImplicitly] get; }

    /// <summary>Gets whether open tracking is enabled.</summary>
    public bool TrackOpens { [UsedImplicitly] get; }

    /// <summary>Gets the link tracking mode.</summary>
    public LinkTracking TrackLinks { [UsedImplicitly] get; }

    /// <summary>Gets whether bounce content is included in webhook payloads.</summary>
    public bool IncludeBounceContentInHook { [UsedImplicitly] get; }

    /// <summary>Gets the click webhook URL.</summary>
    public string? ClickHookUrl { [UsedImplicitly] get; }

    /// <summary>Gets whether SMTP API error webhooks are enabled.</summary>
    public bool EnableSmtpApiErrorHooks { [UsedImplicitly] get; }
}