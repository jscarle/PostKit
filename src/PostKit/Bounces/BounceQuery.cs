using MimeKit;

namespace PostKit.Bounces;

/// <summary>Represents the query parameters used to search Postmark bounces.</summary>
public sealed record BounceQuery
{
    /// <summary>Gets the optional lower bound for the bounce timestamp. PostKit converts the value to Postmark's US Eastern time before sending it.</summary>
    public DateTimeOffset? FromDate { get; init; }

    /// <summary>Gets the optional upper bound for the bounce timestamp. PostKit converts the value to Postmark's US Eastern time before sending it.</summary>
    public DateTimeOffset? ToDate { get; init; }

    /// <summary>Gets the optional bounce type filter.</summary>
    public BounceType? Type { get; init; }

    /// <summary>Gets the optional inactive filter.</summary>
    public bool? Inactive { get; init; }

    /// <summary>Gets the optional recipient email address filter. Only the <see cref="MailboxAddress.Address" /> value is sent to Postmark.</summary>
    public MailboxAddress? EmailFilter { get; init; }

    /// <summary>Gets the optional Postmark message ID filter.</summary>
    public Guid? MessageId { get; init; }

    /// <summary>Gets the optional tag filter.</summary>
    public string? Tag { get; init; }
}
