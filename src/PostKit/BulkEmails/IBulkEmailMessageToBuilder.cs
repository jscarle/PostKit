using MimeKit;

namespace PostKit.BulkEmails;

/// <summary>Provides additional configuration for primary bulk message recipients.</summary>
public interface IBulkEmailMessageToBuilder : IBulkEmailMessageBuilder
{
    /// <summary>Adds another primary recipient by address.</summary>
    /// <param name="address">The recipient email address.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailMessageToBuilder AlsoTo(string address);

    /// <summary>Adds another primary recipient by name and address.</summary>
    /// <param name="name">The display name for the recipient.</param>
    /// <param name="address">The recipient email address.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailMessageToBuilder AlsoTo(string? name, string address);

    /// <summary>Adds another primary recipient using a <see cref="MailboxAddress"/>.</summary>
    /// <param name="mailboxAddress">The recipient mailbox address.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailMessageToBuilder AlsoTo(MailboxAddress mailboxAddress);

    /// <summary>Adds multiple primary recipients by address.</summary>
    /// <param name="addresses">The recipient email addresses.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailMessageToBuilder AlsoTo(IEnumerable<string> addresses);

    /// <summary>Adds multiple primary recipients using mailbox addresses.</summary>
    /// <param name="mailboxAddresses">The recipient mailbox addresses.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailMessageToBuilder AlsoTo(IEnumerable<MailboxAddress> mailboxAddresses);

    /// <summary>Adds multiple primary recipients using mailbox addresses.</summary>
    /// <param name="mailboxAddresses">The recipient mailbox addresses.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailMessageToBuilder AlsoTo(IList<MailboxAddress> mailboxAddresses);
}
