namespace PostKit.Bounces;

/// <summary>Represents the bounce classifications returned by Postmark.</summary>
public enum BounceType
{
    /// <summary>A permanent delivery failure.</summary>
    HardBounce,

    /// <summary>A transient delivery failure.</summary>
    Transient,

    /// <summary>An unsubscribe event recorded as a bounce-type result.</summary>
    Unsubscribe,

    /// <summary>A subscribe event recorded as a bounce-type result.</summary>
    Subscribe,

    /// <summary>An autoresponder message.</summary>
    AutoResponder,

    /// <summary>An address change notification.</summary>
    AddressChange,

    /// <summary>A DNS-related delivery failure.</summary>
    DnsError,

    /// <summary>A spam notification.</summary>
    SpamNotification,

    /// <summary>An open relay test result.</summary>
    OpenRelayTest,

    /// <summary>An unknown bounce classification.</summary>
    Unknown,

    /// <summary>A soft bounce.</summary>
    SoftBounce,

    /// <summary>A virus notification.</summary>
    VirusNotification,

    /// <summary>A MailFrontier Matador classification.</summary>
    MailFrontierMatador,

    /// <summary>A bad email address classification.</summary>
    BadEmailAddress,

    /// <summary>A spam complaint.</summary>
    SpamComplaint,

    /// <summary>A manually deactivated recipient.</summary>
    ManuallyDeactivated,

    /// <summary>An unconfirmed recipient.</summary>
    Unconfirmed,

    /// <summary>A blocked recipient.</summary>
    Blocked,

    /// <summary>An SMTP API error.</summary>
    SmtpApiError,

    /// <summary>An inbound processing error.</summary>
    InboundError,

    /// <summary>A DMARC policy rejection.</summary>
    DmarcPolicy,

    /// <summary>A template rendering failure.</summary>
    TemplateRenderingFailed,
}
