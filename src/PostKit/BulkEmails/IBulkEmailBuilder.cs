using System.Diagnostics.CodeAnalysis;
using MimeKit;
using PostKit.Common;

namespace PostKit.BulkEmails;

/// <summary>Defines a fluent interface for constructing <see cref="BulkEmail"/> requests.</summary>
[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Emails have to go to someone!")]
public interface IBulkEmailBuilder
{
    /// <summary>Finalizes the builder and produces a <see cref="BulkEmail"/> instance.</summary>
    /// <returns>The constructed <see cref="BulkEmail"/> request.</returns>
    BulkEmail Build();

    /// <summary>Sets the sender address shared by the bulk request.</summary>
    /// <param name="address">The sender email address.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailBuilder From(string address);

    /// <summary>Sets the sender using the specified name and address.</summary>
    /// <param name="name">The display name of the sender.</param>
    /// <param name="address">The sender email address.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailBuilder From(string? name, string address);

    /// <summary>Sets the sender using a <see cref="MailboxAddress"/>.</summary>
    /// <param name="mailboxAddress">The sender mailbox address.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailBuilder From(MailboxAddress mailboxAddress);

    /// <summary>Begins configuring reply-to recipients with a single address.</summary>
    /// <param name="address">The reply-to email address.</param>
    /// <returns>An <see cref="IBulkEmailReplyToBuilder"/> for further configuration.</returns>
    IBulkEmailReplyToBuilder ReplyTo(string address);

    /// <summary>Begins configuring reply-to recipients with a name and address.</summary>
    /// <param name="name">The display name for the reply-to recipient.</param>
    /// <param name="address">The reply-to email address.</param>
    /// <returns>An <see cref="IBulkEmailReplyToBuilder"/> for further configuration.</returns>
    IBulkEmailReplyToBuilder ReplyTo(string? name, string address);

    /// <summary>Begins configuring reply-to recipients with a <see cref="MailboxAddress"/>.</summary>
    /// <param name="mailboxAddress">The reply-to mailbox address.</param>
    /// <returns>An <see cref="IBulkEmailReplyToBuilder"/> for further configuration.</returns>
    IBulkEmailReplyToBuilder ReplyTo(MailboxAddress mailboxAddress);

    /// <summary>Begins configuring reply-to recipients with a collection of addresses.</summary>
    /// <param name="addresses">The reply-to email addresses.</param>
    /// <returns>An <see cref="IBulkEmailReplyToBuilder"/> for further configuration.</returns>
    IBulkEmailReplyToBuilder ReplyTo(IEnumerable<string> addresses);

    /// <summary>Begins configuring reply-to recipients with mailbox addresses.</summary>
    /// <param name="mailboxAddresses">The reply-to mailbox addresses.</param>
    /// <returns>An <see cref="IBulkEmailReplyToBuilder"/> for further configuration.</returns>
    IBulkEmailReplyToBuilder ReplyTo(IEnumerable<MailboxAddress> mailboxAddresses);

    /// <summary>Begins configuring reply-to recipients with mailbox addresses.</summary>
    /// <param name="mailboxAddresses">The reply-to mailbox addresses.</param>
    /// <returns>An <see cref="IBulkEmailReplyToBuilder"/> for further configuration.</returns>
    IBulkEmailReplyToBuilder ReplyTo(IList<MailboxAddress> mailboxAddresses);

    /// <summary>Sets the subject shared by every message in the bulk request.</summary>
    /// <param name="subject">The subject text.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailBuilder WithSubject(string subject);

    /// <summary>Sets the shared HTML body content.</summary>
    /// <param name="htmlBody">The HTML body content.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailBuilder WithHtmlBody(string htmlBody);

    /// <summary>Sets the shared plain-text body content.</summary>
    /// <param name="textBody">The text body content.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailBuilder WithTextBody(string textBody);

    /// <summary>Specifies a shared Postmark template by identifier.</summary>
    /// <param name="templateId">The template identifier.</param>
    /// <param name="inlineCss">Whether CSS should be inlined.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailBuilder UsingTemplate(int templateId, bool? inlineCss = null);

    /// <summary>Specifies a shared Postmark template by alias.</summary>
    /// <param name="templateAlias">The template alias.</param>
    /// <param name="inlineCss">Whether CSS should be inlined.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailBuilder UsingTemplate(string templateAlias, bool? inlineCss = null);

    /// <summary>Adds a shared tag to the bulk request.</summary>
    /// <param name="tag">The tag to associate with the request.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailBuilder WithTag(string tag);

    /// <summary>Adds a shared custom header.</summary>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailBuilder WithHeader(string name, string value);

    /// <summary>Adds a shared custom header.</summary>
    /// <param name="header">The header key/value pair.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailBuilder WithHeader(KeyValuePair<string, string> header);

    /// <summary>Adds shared custom headers.</summary>
    /// <param name="headers">The collection of header key/value pairs.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailBuilder WithHeaders(IEnumerable<KeyValuePair<string, string>> headers);

    /// <summary>Adds shared custom headers.</summary>
    /// <param name="headers">The dictionary of header names and values.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailBuilder WithHeaders(IDictionary<string, string> headers);

    /// <summary>Adds shared metadata.</summary>
    /// <param name="name">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailBuilder WithMetadata(string name, string value);

    /// <summary>Adds shared metadata.</summary>
    /// <param name="entry">The metadata key/value pair.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailBuilder WithMetadata(KeyValuePair<string, string> entry);

    /// <summary>Adds shared metadata entries.</summary>
    /// <param name="metadata">The collection of metadata entries.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailBuilder WithMetadata(IEnumerable<KeyValuePair<string, string>> metadata);

    /// <summary>Adds shared metadata entries.</summary>
    /// <param name="metadata">The dictionary of metadata keys and values.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailBuilder WithMetadata(IDictionary<string, string> metadata);

    /// <summary>Enables or disables open tracking for the bulk request.</summary>
    /// <param name="openTracking">A value indicating whether open tracking is enabled.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailBuilder WithOpenTracking(bool openTracking = true);

    /// <summary>Configures link tracking for the bulk request.</summary>
    /// <param name="linkTracking">The link tracking mode to use.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailBuilder WithLinkTracking(LinkTracking linkTracking = LinkTracking.HtmlAndText);

    /// <summary>Specifies the message stream for the bulk request.</summary>
    /// <param name="messageStream">The <see cref="MessageStream"/> to use.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailBuilder UsingMessageStream(MessageStream messageStream);

    /// <summary>Specifies the message stream for the bulk request using an identifier.</summary>
    /// <param name="messageStreamId">The message stream identifier.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailBuilder UsingMessageStream(string messageStreamId);

    /// <summary>Attaches a shared file to the bulk request.</summary>
    /// <param name="attachment">The attachment to include.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailBuilder WithAttachment(Attachment attachment);

    /// <summary>Attaches shared files to the bulk request.</summary>
    /// <param name="attachments">The attachments to include.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailBuilder WithAttachments(IEnumerable<Attachment> attachments);

    /// <summary>Adds a recipient-specific message to the bulk request.</summary>
    /// <param name="message">The message to include.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailBuilder AddMessage(BulkEmailMessage message);

    /// <summary>Adds recipient-specific messages to the bulk request.</summary>
    /// <param name="messages">The messages to include.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailBuilder AddMessages(IEnumerable<BulkEmailMessage> messages);
}
