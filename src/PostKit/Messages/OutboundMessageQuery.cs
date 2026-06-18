using MimeKit;

namespace PostKit.Messages;

/// <summary>Represents the query parameters used to search outbound Postmark messages.</summary>
public sealed record OutboundMessageQuery
{
    /// <summary>Gets the optional lower bound for the message timestamp. PostKit converts the value to Postmark's US Eastern time before sending it.</summary>
    public DateTimeOffset? FromDate { get; init; }

    /// <summary>Gets the optional upper bound for the message timestamp. PostKit converts the value to Postmark's US Eastern time before sending it.</summary>
    public DateTimeOffset? ToDate { get; init; }

    /// <summary>Gets the optional recipient email address filter. Only the <see cref="MailboxAddress.Address" /> value is sent to Postmark.</summary>
    public MailboxAddress? Recipient { get; init; }

    /// <summary>Gets the optional sender email address filter. Only the <see cref="MailboxAddress.Address" /> value is sent to Postmark.</summary>
    public MailboxAddress? FromEmail { get; init; }

    /// <summary>Gets the optional tag filter.</summary>
    public string? Tag { get; init; }

    /// <summary>Gets the optional outbound message status filter.</summary>
    public OutboundMessageStatus? Status { get; init; }

    /// <summary>Gets the optional email subject filter.</summary>
    public string? Subject { get; init; }

    /// <summary>Gets the optional metadata filter. Postmark currently supports searching by only one metadata field at a time.</summary>
    public OutboundMessageMetadataFilter? Metadata { get; init; }
}
