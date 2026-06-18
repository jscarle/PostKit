using JetBrains.Annotations;
using MimeKit;

namespace PostKit.Messages;

/// <summary>Represents filters for searching inbound messages.</summary>
public sealed record InboundMessageQuery
{
    /// <summary>Gets the optional start date filter.</summary>
    public DateTimeOffset? FromDate { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional end date filter.</summary>
    public DateTimeOffset? ToDate { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional recipient filter.</summary>
    public MailboxAddress? Recipient { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional sender email filter.</summary>
    public MailboxAddress? FromEmail { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional tag filter.</summary>
    public string? Tag { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional subject filter.</summary>
    public string? Subject { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional mailbox hash filter.</summary>
    public string? MailboxHash { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional status filter.</summary>
    public InboundMessageStatus? Status { [UsedImplicitly] get; init; }
}
