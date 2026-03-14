using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using MimeKit;

namespace PostKit.BulkEmails;

/// <summary>Defines a fluent interface for constructing <see cref="BulkEmailMessage"/> values.</summary>
[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Emails have to go to someone!")]
public interface IBulkEmailMessageBuilder
{
    /// <summary>Finalizes the builder and produces a <see cref="BulkEmailMessage"/> instance.</summary>
    /// <returns>The constructed <see cref="BulkEmailMessage"/>.</returns>
    BulkEmailMessage Build();

    /// <summary>Begins configuring primary recipients with a single address.</summary>
    /// <param name="address">The recipient email address.</param>
    /// <returns>An <see cref="IBulkEmailMessageToBuilder"/> for further configuration.</returns>
    IBulkEmailMessageToBuilder To(string address);

    /// <summary>Begins configuring primary recipients with a name and address.</summary>
    /// <param name="name">The display name for the recipient.</param>
    /// <param name="address">The recipient email address.</param>
    /// <returns>An <see cref="IBulkEmailMessageToBuilder"/> for further configuration.</returns>
    IBulkEmailMessageToBuilder To(string? name, string address);

    /// <summary>Begins configuring primary recipients with a <see cref="MailboxAddress"/>.</summary>
    /// <param name="mailboxAddress">The recipient mailbox address.</param>
    /// <returns>An <see cref="IBulkEmailMessageToBuilder"/> for further configuration.</returns>
    IBulkEmailMessageToBuilder To(MailboxAddress mailboxAddress);

    /// <summary>Begins configuring primary recipients with a collection of addresses.</summary>
    /// <param name="addresses">The recipient email addresses.</param>
    /// <returns>An <see cref="IBulkEmailMessageToBuilder"/> for further configuration.</returns>
    IBulkEmailMessageToBuilder To(IEnumerable<string> addresses);

    /// <summary>Begins configuring primary recipients with mailbox addresses.</summary>
    /// <param name="mailboxAddresses">The recipient mailbox addresses.</param>
    /// <returns>An <see cref="IBulkEmailMessageToBuilder"/> for further configuration.</returns>
    IBulkEmailMessageToBuilder To(IEnumerable<MailboxAddress> mailboxAddresses);

    /// <summary>Begins configuring primary recipients with mailbox addresses.</summary>
    /// <param name="mailboxAddresses">The recipient mailbox addresses.</param>
    /// <returns>An <see cref="IBulkEmailMessageToBuilder"/> for further configuration.</returns>
    IBulkEmailMessageToBuilder To(IList<MailboxAddress> mailboxAddresses);

    /// <summary>Begins configuring carbon-copy recipients with a single address.</summary>
    /// <param name="address">The CC email address.</param>
    /// <returns>An <see cref="IBulkEmailMessageCcBuilder"/> for further configuration.</returns>
    IBulkEmailMessageCcBuilder Cc(string address);

    /// <summary>Begins configuring carbon-copy recipients with a name and address.</summary>
    /// <param name="name">The display name for the CC recipient.</param>
    /// <param name="address">The CC email address.</param>
    /// <returns>An <see cref="IBulkEmailMessageCcBuilder"/> for further configuration.</returns>
    IBulkEmailMessageCcBuilder Cc(string? name, string address);

    /// <summary>Begins configuring carbon-copy recipients with a <see cref="MailboxAddress"/>.</summary>
    /// <param name="mailboxAddress">The CC mailbox address.</param>
    /// <returns>An <see cref="IBulkEmailMessageCcBuilder"/> for further configuration.</returns>
    IBulkEmailMessageCcBuilder Cc(MailboxAddress mailboxAddress);

    /// <summary>Begins configuring carbon-copy recipients with a collection of addresses.</summary>
    /// <param name="addresses">The CC email addresses.</param>
    /// <returns>An <see cref="IBulkEmailMessageCcBuilder"/> for further configuration.</returns>
    IBulkEmailMessageCcBuilder Cc(IEnumerable<string> addresses);

    /// <summary>Begins configuring carbon-copy recipients with mailbox addresses.</summary>
    /// <param name="mailboxAddresses">The CC mailbox addresses.</param>
    /// <returns>An <see cref="IBulkEmailMessageCcBuilder"/> for further configuration.</returns>
    IBulkEmailMessageCcBuilder Cc(IEnumerable<MailboxAddress> mailboxAddresses);

    /// <summary>Begins configuring carbon-copy recipients with mailbox addresses.</summary>
    /// <param name="mailboxAddresses">The CC mailbox addresses.</param>
    /// <returns>An <see cref="IBulkEmailMessageCcBuilder"/> for further configuration.</returns>
    IBulkEmailMessageCcBuilder Cc(IList<MailboxAddress> mailboxAddresses);

    /// <summary>Begins configuring blind carbon-copy recipients with a single address.</summary>
    /// <param name="address">The BCC email address.</param>
    /// <returns>An <see cref="IBulkEmailMessageBccBuilder"/> for further configuration.</returns>
    IBulkEmailMessageBccBuilder Bcc(string address);

    /// <summary>Begins configuring blind carbon-copy recipients with a name and address.</summary>
    /// <param name="name">The display name for the BCC recipient.</param>
    /// <param name="address">The BCC email address.</param>
    /// <returns>An <see cref="IBulkEmailMessageBccBuilder"/> for further configuration.</returns>
    IBulkEmailMessageBccBuilder Bcc(string? name, string address);

    /// <summary>Begins configuring blind carbon-copy recipients with a <see cref="MailboxAddress"/>.</summary>
    /// <param name="mailboxAddress">The BCC mailbox address.</param>
    /// <returns>An <see cref="IBulkEmailMessageBccBuilder"/> for further configuration.</returns>
    IBulkEmailMessageBccBuilder Bcc(MailboxAddress mailboxAddress);

    /// <summary>Begins configuring blind carbon-copy recipients with a collection of addresses.</summary>
    /// <param name="addresses">The BCC email addresses.</param>
    /// <returns>An <see cref="IBulkEmailMessageBccBuilder"/> for further configuration.</returns>
    IBulkEmailMessageBccBuilder Bcc(IEnumerable<string> addresses);

    /// <summary>Begins configuring blind carbon-copy recipients with mailbox addresses.</summary>
    /// <param name="mailboxAddresses">The BCC mailbox addresses.</param>
    /// <returns>An <see cref="IBulkEmailMessageBccBuilder"/> for further configuration.</returns>
    IBulkEmailMessageBccBuilder Bcc(IEnumerable<MailboxAddress> mailboxAddresses);

    /// <summary>Begins configuring blind carbon-copy recipients with mailbox addresses.</summary>
    /// <param name="mailboxAddresses">The BCC mailbox addresses.</param>
    /// <returns>An <see cref="IBulkEmailMessageBccBuilder"/> for further configuration.</returns>
    IBulkEmailMessageBccBuilder Bcc(IList<MailboxAddress> mailboxAddresses);

    /// <summary>Sets the recipient-specific template model.</summary>
    /// <param name="templateModel">The template model data.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailMessageBuilder WithTemplateModel(object templateModel);

    /// <summary>Sets the recipient-specific template model using explicit serializer options for this call.</summary>
    /// <param name="templateModel">The template model data.</param>
    /// <param name="serializerOptions">The serializer options to use for this template model.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailMessageBuilder WithTemplateModel(object templateModel, JsonSerializerOptions serializerOptions);

    /// <summary>Adds recipient-specific metadata.</summary>
    /// <param name="name">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailMessageBuilder WithMetadata(string name, string value);

    /// <summary>Adds recipient-specific metadata.</summary>
    /// <param name="entry">The metadata key/value pair.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailMessageBuilder WithMetadata(KeyValuePair<string, string> entry);

    /// <summary>Adds recipient-specific metadata entries.</summary>
    /// <param name="metadata">The collection of metadata entries.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailMessageBuilder WithMetadata(IEnumerable<KeyValuePair<string, string>> metadata);

    /// <summary>Adds recipient-specific metadata entries.</summary>
    /// <param name="metadata">The dictionary of metadata keys and values.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailMessageBuilder WithMetadata(IDictionary<string, string> metadata);

    /// <summary>Adds a recipient-specific custom header.</summary>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailMessageBuilder WithHeader(string name, string value);

    /// <summary>Adds a recipient-specific custom header.</summary>
    /// <param name="header">The header key/value pair.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailMessageBuilder WithHeader(KeyValuePair<string, string> header);

    /// <summary>Adds recipient-specific custom headers.</summary>
    /// <param name="headers">The collection of header key/value pairs.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailMessageBuilder WithHeaders(IEnumerable<KeyValuePair<string, string>> headers);

    /// <summary>Adds recipient-specific custom headers.</summary>
    /// <param name="headers">The dictionary of header names and values.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    IBulkEmailMessageBuilder WithHeaders(IDictionary<string, string> headers);
}
