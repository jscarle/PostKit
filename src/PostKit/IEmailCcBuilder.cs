using MimeKit;

namespace PostKit;

/// <summary>Provides additional configuration for carbon-copy email recipients.</summary>
public interface IEmailCcBuilder : IEmailBuilder
{
    /// <summary>Adds another carbon-copy recipient by address.</summary>
    /// <param name="address">The CC email address.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailCcBuilder AlsoCc(string address);

    /// <summary>Adds another carbon-copy recipient by name and address.</summary>
    /// <param name="name">The display name for the CC recipient.</param>
    /// <param name="address">The CC email address.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailCcBuilder AlsoCc(string name, string address);

    /// <summary>Adds another carbon-copy recipient using a <see cref="MailboxAddress"/>.</summary>
    /// <param name="mailboxAddress">The CC mailbox address.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailCcBuilder AlsoCc(MailboxAddress mailboxAddress);

    /// <summary>Adds multiple carbon-copy recipients by address.</summary>
    /// <param name="addresses">The CC email addresses.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailCcBuilder AlsoCc(IEnumerable<string> addresses);

    /// <summary>Adds multiple carbon-copy recipients using mailbox addresses.</summary>
    /// <param name="mailboxAddresses">The CC mailbox addresses.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailCcBuilder AlsoCc(IEnumerable<MailboxAddress> mailboxAddresses);

    /// <summary>Adds multiple carbon-copy recipients using mailbox addresses.</summary>
    /// <param name="mailboxAddresses">The CC mailbox addresses.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailCcBuilder AlsoCc(IList<MailboxAddress> mailboxAddresses);
}
