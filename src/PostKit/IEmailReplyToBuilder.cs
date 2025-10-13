using MimeKit;

namespace PostKit;

/// <summary>Provides additional configuration for reply-to email recipients.</summary>
public interface IEmailReplyToBuilder : IEmailBuilder
{
    /// <summary>Adds another reply-to recipient by address.</summary>
    /// <param name="address">The reply-to email address.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailReplyToBuilder AlsoReplyTo(string address);

    /// <summary>Adds another reply-to recipient by name and address.</summary>
    /// <param name="name">The display name for the reply-to recipient.</param>
    /// <param name="address">The reply-to email address.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailReplyToBuilder AlsoReplyTo(string name, string address);

    /// <summary>Adds another reply-to recipient using a <see cref="MailboxAddress"/>.</summary>
    /// <param name="mailboxAddress">The reply-to mailbox address.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailReplyToBuilder AlsoReplyTo(MailboxAddress mailboxAddress);

    /// <summary>Adds multiple reply-to recipients by address.</summary>
    /// <param name="addresses">The reply-to email addresses.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailReplyToBuilder AlsoReplyTo(IEnumerable<string> addresses);

    /// <summary>Adds multiple reply-to recipients using mailbox addresses.</summary>
    /// <param name="mailboxAddresses">The reply-to mailbox addresses.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailReplyToBuilder AlsoReplyTo(IEnumerable<MailboxAddress> mailboxAddresses);

    /// <summary>Adds multiple reply-to recipients using mailbox addresses.</summary>
    /// <param name="mailboxAddresses">The reply-to mailbox addresses.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailReplyToBuilder AlsoReplyTo(IList<MailboxAddress> mailboxAddresses);
}
