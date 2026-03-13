using MimeKit;

namespace PostKit.Bounces;

/// <summary>Represents the query parameters used to search Postmark bounces.</summary>
public sealed record BounceQuery
{
    /// <summary>Gets the number of bounces to return. Postmark currently allows between 1 and 500.</summary>
    public required int Count { get; init; }

    /// <summary>Gets the number of bounces to skip before returning results.</summary>
    public int Offset { get; init; }

    /// <summary>Gets the optional bounce type filter.</summary>
    public BounceType? Type { get; init; }

    /// <summary>Gets the optional inactive filter.</summary>
    public bool? Inactive { get; init; }

    /// <summary>Gets the optional recipient email address filter. Only the <see cref="MailboxAddress.Address"/> value is sent to Postmark.</summary>
    public MailboxAddress? EmailFilter { get; init; }

    /// <summary>Gets the optional Postmark message ID filter.</summary>
    public Guid? MessageId { get; init; }

    /// <summary>Gets the optional tag filter.</summary>
    public string? Tag { get; init; }

    /// <summary>Gets the optional upper bound for the bounce timestamp.
    /// Postmark accepts either a date (`yyyy-MM-dd`) or a timestamp up to seconds (`yyyy-MM-ddTHH:mm:ss`) and interprets it using US Eastern time.</summary>
    public DateTime? ToDate { get; init; }

    /// <summary>Gets the optional lower bound for the bounce timestamp.
    /// Postmark accepts either a date (`yyyy-MM-dd`) or a timestamp up to seconds (`yyyy-MM-ddTHH:mm:ss`) and interprets it using US Eastern time.</summary>
    public DateTime? FromDate { get; init; }

    /// <summary>Gets the optional message stream ID filter.</summary>
    public string? MessageStream { get; init; }
}
