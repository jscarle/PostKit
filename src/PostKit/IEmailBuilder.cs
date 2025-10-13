using System.Diagnostics.CodeAnalysis;
using MimeKit;

namespace PostKit;

[SuppressMessage("Naming", "CA1716: Identifiers should not match keywords", Justification = "Emails have to go to someone!")]
/// <summary>
/// Provides a fluent interface for constructing <see cref="Email"/> messages.
/// </summary>
public interface IEmailBuilder
{
    /// <summary>
    /// Finalizes the builder and produces an <see cref="Email"/> instance.
    /// </summary>
    /// <returns>The constructed <see cref="Email"/>.</returns>
    Email Build();

    /// <summary>
    /// Sets the sender address for the email.
    /// </summary>
    /// <param name="address">The sender email address.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailBuilder From(string address);

    /// <summary>
    /// Sets the sender using the specified name and address.
    /// </summary>
    /// <param name="name">The display name of the sender.</param>
    /// <param name="address">The sender email address.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailBuilder From(string name, string address);

    /// <summary>
    /// Sets the sender using a <see cref="MailboxAddress"/>.
    /// </summary>
    /// <param name="mailboxAddress">The sender mailbox address.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailBuilder From(MailboxAddress mailboxAddress);

    /// <summary>
    /// Begins configuring reply-to recipients with a single address.
    /// </summary>
    /// <param name="address">The reply-to email address.</param>
    /// <returns>An <see cref="IEmailReplyToBuilder"/> for further configuration.</returns>
    IEmailReplyToBuilder ReplyTo(string address);

    /// <summary>
    /// Begins configuring reply-to recipients with a name and address.
    /// </summary>
    /// <param name="name">The display name for the reply-to recipient.</param>
    /// <param name="address">The reply-to email address.</param>
    /// <returns>An <see cref="IEmailReplyToBuilder"/> for further configuration.</returns>
    IEmailReplyToBuilder ReplyTo(string name, string address);

    /// <summary>
    /// Begins configuring reply-to recipients with a <see cref="MailboxAddress"/>.
    /// </summary>
    /// <param name="mailboxAddress">The reply-to mailbox address.</param>
    /// <returns>An <see cref="IEmailReplyToBuilder"/> for further configuration.</returns>
    IEmailReplyToBuilder ReplyTo(MailboxAddress mailboxAddress);

    /// <summary>
    /// Begins configuring reply-to recipients with a collection of addresses.
    /// </summary>
    /// <param name="addresses">The reply-to email addresses.</param>
    /// <returns>An <see cref="IEmailReplyToBuilder"/> for further configuration.</returns>
    IEmailReplyToBuilder ReplyTo(IEnumerable<string> addresses);

    /// <summary>
    /// Begins configuring reply-to recipients with mailbox addresses.
    /// </summary>
    /// <param name="mailboxAddresses">The reply-to mailbox addresses.</param>
    /// <returns>An <see cref="IEmailReplyToBuilder"/> for further configuration.</returns>
    IEmailReplyToBuilder ReplyTo(IList<MailboxAddress> mailboxAddresses);

    /// <summary>
    /// Begins configuring primary recipients with a single address.
    /// </summary>
    /// <param name="address">The recipient email address.</param>
    /// <returns>An <see cref="IEmailToBuilder"/> for further configuration.</returns>
    IEmailToBuilder To(string address);

    /// <summary>
    /// Begins configuring primary recipients with a name and address.
    /// </summary>
    /// <param name="name">The display name for the recipient.</param>
    /// <param name="address">The recipient email address.</param>
    /// <returns>An <see cref="IEmailToBuilder"/> for further configuration.</returns>
    IEmailToBuilder To(string name, string address);

    /// <summary>
    /// Begins configuring primary recipients with a <see cref="MailboxAddress"/>.
    /// </summary>
    /// <param name="mailboxAddress">The recipient mailbox address.</param>
    /// <returns>An <see cref="IEmailToBuilder"/> for further configuration.</returns>
    IEmailToBuilder To(MailboxAddress mailboxAddress);

    /// <summary>
    /// Begins configuring primary recipients with a collection of addresses.
    /// </summary>
    /// <param name="addresses">The recipient email addresses.</param>
    /// <returns>An <see cref="IEmailToBuilder"/> for further configuration.</returns>
    IEmailToBuilder To(IEnumerable<string> addresses);

    /// <summary>
    /// Begins configuring primary recipients with mailbox addresses.
    /// </summary>
    /// <param name="mailboxAddresses">The recipient mailbox addresses.</param>
    /// <returns>An <see cref="IEmailToBuilder"/> for further configuration.</returns>
    IEmailToBuilder To(IList<MailboxAddress> mailboxAddresses);

    /// <summary>
    /// Begins configuring carbon-copy recipients with a single address.
    /// </summary>
    /// <param name="address">The CC email address.</param>
    /// <returns>An <see cref="IEmailCcBuilder"/> for further configuration.</returns>
    IEmailCcBuilder Cc(string address);

    /// <summary>
    /// Begins configuring carbon-copy recipients with a name and address.
    /// </summary>
    /// <param name="name">The display name for the CC recipient.</param>
    /// <param name="address">The CC email address.</param>
    /// <returns>An <see cref="IEmailCcBuilder"/> for further configuration.</returns>
    IEmailCcBuilder Cc(string name, string address);

    /// <summary>
    /// Begins configuring carbon-copy recipients with a <see cref="MailboxAddress"/>.
    /// </summary>
    /// <param name="mailboxAddress">The CC mailbox address.</param>
    /// <returns>An <see cref="IEmailCcBuilder"/> for further configuration.</returns>
    IEmailCcBuilder Cc(MailboxAddress mailboxAddress);

    /// <summary>
    /// Begins configuring carbon-copy recipients with a collection of addresses.
    /// </summary>
    /// <param name="addresses">The CC email addresses.</param>
    /// <returns>An <see cref="IEmailCcBuilder"/> for further configuration.</returns>
    IEmailCcBuilder Cc(IEnumerable<string> addresses);

    /// <summary>
    /// Begins configuring carbon-copy recipients with mailbox addresses.
    /// </summary>
    /// <param name="mailboxAddresses">The CC mailbox addresses.</param>
    /// <returns>An <see cref="IEmailCcBuilder"/> for further configuration.</returns>
    IEmailCcBuilder Cc(IList<MailboxAddress> mailboxAddresses);

    /// <summary>
    /// Begins configuring blind carbon-copy recipients with a single address.
    /// </summary>
    /// <param name="address">The BCC email address.</param>
    /// <returns>An <see cref="IEmailBccBuilder"/> for further configuration.</returns>
    IEmailBccBuilder Bcc(string address);

    /// <summary>
    /// Begins configuring blind carbon-copy recipients with a name and address.
    /// </summary>
    /// <param name="name">The display name for the BCC recipient.</param>
    /// <param name="address">The BCC email address.</param>
    /// <returns>An <see cref="IEmailBccBuilder"/> for further configuration.</returns>
    IEmailBccBuilder Bcc(string name, string address);

    /// <summary>
    /// Begins configuring blind carbon-copy recipients with a <see cref="MailboxAddress"/>.
    /// </summary>
    /// <param name="mailboxAddress">The BCC mailbox address.</param>
    /// <returns>An <see cref="IEmailBccBuilder"/> for further configuration.</returns>
    IEmailBccBuilder Bcc(MailboxAddress mailboxAddress);

    /// <summary>
    /// Begins configuring blind carbon-copy recipients with a collection of addresses.
    /// </summary>
    /// <param name="addresses">The BCC email addresses.</param>
    /// <returns>An <see cref="IEmailBccBuilder"/> for further configuration.</returns>
    IEmailBccBuilder Bcc(IEnumerable<string> addresses);

    /// <summary>
    /// Begins configuring blind carbon-copy recipients with mailbox addresses.
    /// </summary>
    /// <param name="mailboxAddresses">The BCC mailbox addresses.</param>
    /// <returns>An <see cref="IEmailBccBuilder"/> for further configuration.</returns>
    IEmailBccBuilder Bcc(IList<MailboxAddress> mailboxAddresses);

    /// <summary>
    /// Sets the subject line for the email.
    /// </summary>
    /// <param name="subject">The subject text.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailBuilder WithSubject(string subject);

    /// <summary>
    /// Sets the HTML body content of the email.
    /// </summary>
    /// <param name="html">The HTML body content.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailBuilder WithHtmlBody(string html);

    /// <summary>
    /// Sets the plain-text body content of the email.
    /// </summary>
    /// <param name="text">The text body content.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailBuilder WithTextBody(string text);

    /// <summary>
    /// Adds a tag to the email.
    /// </summary>
    /// <param name="tag">The tag to associate with the email.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailBuilder WithTag(string tag);

    /// <summary>
    /// Adds a custom header to the email.
    /// </summary>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailBuilder WithHeader(string name, string value);

    /// <summary>
    /// Adds a custom header to the email.
    /// </summary>
    /// <param name="header">The header key/value pair.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailBuilder WithHeader(KeyValuePair<string, string> header);

    /// <summary>
    /// Adds custom headers to the email.
    /// </summary>
    /// <param name="headers">The collection of header key/value pairs.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailBuilder WithHeader(IEnumerable<KeyValuePair<string, string>> headers);

    /// <summary>
    /// Adds custom headers to the email.
    /// </summary>
    /// <param name="headers">The dictionary of header names and values.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailBuilder WithHeader(IDictionary<string, string> headers);

    /// <summary>
    /// Adds metadata to the email.
    /// </summary>
    /// <param name="name">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailBuilder WithMetadata(string name, string value);

    /// <summary>
    /// Adds metadata to the email.
    /// </summary>
    /// <param name="entry">The metadata key/value pair.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailBuilder WithMetadata(KeyValuePair<string, string> entry);

    /// <summary>
    /// Adds metadata entries to the email.
    /// </summary>
    /// <param name="metadata">The collection of metadata entries.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailBuilder WithMetadata(IEnumerable<KeyValuePair<string, string>> metadata);

    /// <summary>
    /// Adds metadata entries to the email.
    /// </summary>
    /// <param name="metadata">The dictionary of metadata keys and values.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailBuilder WithMetadata(IDictionary<string, string> metadata);

    /// <summary>
    /// Attaches a file to the email.
    /// </summary>
    /// <param name="attachment">The attachment to include.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailBuilder WithAttachment(Attachment attachment);

    /// <summary>
    /// Attaches files to the email.
    /// </summary>
    /// <param name="attachments">The attachments to include.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailBuilder WithAttachments(IEnumerable<Attachment> attachments);

    /// <summary>
    /// Enables or disables open tracking for the email.
    /// </summary>
    /// <param name="openTracking">A value indicating whether open tracking is enabled.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailBuilder WithOpenTracking(bool openTracking = true);

    /// <summary>
    /// Configures link tracking for the email.
    /// </summary>
    /// <param name="linkTracking">The link tracking mode to use.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailBuilder WithLinkTracking(LinkTracking linkTracking = LinkTracking.HtmlAndText);

    /// <summary>
    /// Specifies the message stream for the email.
    /// </summary>
    /// <param name="messageStream">The <see cref="MessageStream"/> to use.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailBuilder UsingMessageStream(MessageStream messageStream);

    /// <summary>
    /// Specifies the message stream for the email using an identifier.
    /// </summary>
    /// <param name="messageStreamId">The message stream identifier.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailBuilder UsingMessageStream(string messageStreamId);

    /// <summary>
    /// Applies a template to the email using a numeric identifier.
    /// </summary>
    /// <param name="templateId">The template identifier.</param>
    /// <param name="templateModel">The template model data.</param>
    /// <param name="inlineCss">Whether CSS should be inlined.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailBuilder WithTemplate(int templateId, object templateModel, bool? inlineCss = null);

    /// <summary>
    /// Applies a template to the email using an alias.
    /// </summary>
    /// <param name="templateAlias">The template alias.</param>
    /// <param name="templateModel">The template model data.</param>
    /// <param name="inlineCss">Whether CSS should be inlined.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IEmailBuilder WithTemplate(string templateAlias, object templateModel, bool? inlineCss = null);
}
