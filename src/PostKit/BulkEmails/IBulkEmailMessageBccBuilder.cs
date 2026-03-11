using MimeKit;

namespace PostKit.BulkEmails;

/// <summary>Provides additional configuration for blind carbon-copy bulk message recipients.</summary>
public interface IBulkEmailMessageBccBuilder : IBulkEmailMessageBuilder
{
    /// <summary>Adds another blind carbon-copy recipient by address.</summary>
    /// <param name="address">The BCC email address.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailMessageBccBuilder AlsoBcc(string address);

    /// <summary>Adds another blind carbon-copy recipient by name and address.</summary>
    /// <param name="name">The display name for the BCC recipient.</param>
    /// <param name="address">The BCC email address.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailMessageBccBuilder AlsoBcc(string? name, string address);

    /// <summary>Adds another blind carbon-copy recipient using a <see cref="MailboxAddress"/>.</summary>
    /// <param name="mailboxAddress">The BCC mailbox address.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailMessageBccBuilder AlsoBcc(MailboxAddress mailboxAddress);

    /// <summary>Adds multiple blind carbon-copy recipients by address.</summary>
    /// <param name="addresses">The BCC email addresses.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailMessageBccBuilder AlsoBcc(IEnumerable<string> addresses);

    /// <summary>Adds multiple blind carbon-copy recipients using mailbox addresses.</summary>
    /// <param name="mailboxAddresses">The BCC mailbox addresses.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailMessageBccBuilder AlsoBcc(IEnumerable<MailboxAddress> mailboxAddresses);

    /// <summary>Adds multiple blind carbon-copy recipients using mailbox addresses.</summary>
    /// <param name="mailboxAddresses">The BCC mailbox addresses.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailMessageBccBuilder AlsoBcc(IList<MailboxAddress> mailboxAddresses);
}
