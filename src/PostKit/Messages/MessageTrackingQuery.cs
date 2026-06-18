using JetBrains.Annotations;
using MimeKit;

namespace PostKit.Messages;

/// <summary>Represents filters for searching message open or click events.</summary>
public sealed record MessageTrackingQuery
{
    /// <summary>Gets the optional recipient filter. Only the <see cref="MailboxAddress.Address" /> value is sent to Postmark.</summary>
    public MailboxAddress? Recipient { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional tag filter.</summary>
    public string? Tag { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional client name filter.</summary>
    public string? ClientName { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional client company filter.</summary>
    public string? ClientCompany { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional client family filter.</summary>
    public string? ClientFamily { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional operating system name filter.</summary>
    public string? OsName { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional operating system family filter.</summary>
    public string? OsFamily { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional operating system company filter.</summary>
    public string? OsCompany { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional platform filter.</summary>
    public string? Platform { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional country filter.</summary>
    public string? Country { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional region filter.</summary>
    public string? Region { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional city filter.</summary>
    public string? City { [UsedImplicitly] get; init; }
}
