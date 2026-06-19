namespace PostKit.Bounces;

/// <summary>Represents the bounce classifications returned by Postmark.</summary>
public enum BounceType
{
    /// <summary>A permanent delivery failure.</summary>
    HardBounce = 1,

    /// <summary>A transient delivery failure.</summary>
    Transient = 2,

    /// <summary>An unsubscribe event recorded as a bounce-type result.</summary>
    Unsubscribe = 16,

    /// <summary>A subscribe event recorded as a bounce-type result.</summary>
    Subscribe = 32,

    /// <summary>An autoresponder message.</summary>
    AutoResponder = 64,

    /// <summary>An address change notification.</summary>
    AddressChange = 128,

    /// <summary>A DNS-related delivery failure.</summary>
    DnsError = 256,

    /// <summary>A spam notification.</summary>
    SpamNotification = 512,

    /// <summary>An open relay test result.</summary>
    OpenRelayTest = 1024,

    /// <summary>An unknown bounce classification.</summary>
    Unknown = 2048,

    /// <summary>A soft bounce.</summary>
    SoftBounce = 4096,

    /// <summary>A virus notification.</summary>
    VirusNotification = 8192,

    /// <summary>A spam challenge verification response.</summary>
    ChallengeVerification = 16384,

    /// <summary>A bad email address classification.</summary>
    BadEmailAddress = 100000,

    /// <summary>A spam complaint.</summary>
    SpamComplaint = 100001,

    /// <summary>A manually deactivated recipient.</summary>
    ManuallyDeactivated = 100002,

    /// <summary>An unconfirmed recipient.</summary>
    Unconfirmed = 100003,

    /// <summary>A blocked recipient.</summary>
    Blocked = 100006,

    /// <summary>An SMTP API error.</summary>
    SmtpApiError = 100007,

    /// <summary>An inbound processing error.</summary>
    InboundError = 100008,

    /// <summary>A DMARC policy rejection.</summary>
    DmarcPolicy = 100009,

    /// <summary>A template rendering failure.</summary>
    TemplateRenderingFailed = 100010
}