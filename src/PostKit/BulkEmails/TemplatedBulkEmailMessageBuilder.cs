using System.Text.Json;
using MimeKit;

namespace PostKit.BulkEmails;

/// <summary>Provides a fluent interface for composing a <see cref="BulkEmailMessage" /> with a template model.</summary>
public sealed class TemplatedBulkEmailMessageBuilder
{
    private readonly DraftBulkEmailMessage _draft = new();

    internal TemplatedBulkEmailMessageBuilder()
    {
    }

    /// <summary>Adds a primary recipient to the message.</summary>
    /// <param name="address">The recipient email address.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailMessageBuilder To(string address)
    {
        _draft.To(address);
        return this;
    }

    /// <summary>Adds a primary recipient to the message.</summary>
    /// <param name="address">The recipient email address.</param>
    /// <param name="name">The optional recipient display name.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailMessageBuilder To(string address, string? name)
    {
        _draft.To(address, name);
        return this;
    }

    /// <summary>Adds a primary recipient to the message.</summary>
    /// <param name="mailboxAddress">The recipient mailbox address.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailMessageBuilder To(MailboxAddress mailboxAddress)
    {
        _draft.To(mailboxAddress);
        return this;
    }

    /// <summary>Adds primary recipients to the message.</summary>
    /// <param name="addresses">The recipient email addresses.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailMessageBuilder To(IEnumerable<string> addresses)
    {
        _draft.To(addresses);
        return this;
    }

    /// <summary>Adds primary recipients to the message.</summary>
    /// <param name="mailboxAddresses">The recipient mailbox addresses.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailMessageBuilder To(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _draft.To(mailboxAddresses);
        return this;
    }

    /// <summary>Adds primary recipients to the message.</summary>
    /// <param name="mailboxAddresses">The recipient mailbox addresses.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailMessageBuilder To(IList<MailboxAddress> mailboxAddresses)
    {
        _draft.To(mailboxAddresses);
        return this;
    }

    /// <summary>Adds a carbon-copy recipient to the message.</summary>
    /// <param name="address">The carbon-copy recipient email address.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailMessageBuilder Cc(string address)
    {
        _draft.Cc(address);
        return this;
    }

    /// <summary>Adds a carbon-copy recipient to the message.</summary>
    /// <param name="address">The carbon-copy recipient email address.</param>
    /// <param name="name">The optional carbon-copy recipient display name.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailMessageBuilder Cc(string address, string? name)
    {
        _draft.Cc(address, name);
        return this;
    }

    /// <summary>Adds a carbon-copy recipient to the message.</summary>
    /// <param name="mailboxAddress">The carbon-copy recipient mailbox address.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailMessageBuilder Cc(MailboxAddress mailboxAddress)
    {
        _draft.Cc(mailboxAddress);
        return this;
    }

    /// <summary>Adds carbon-copy recipients to the message.</summary>
    /// <param name="addresses">The carbon-copy recipient email addresses.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailMessageBuilder Cc(IEnumerable<string> addresses)
    {
        _draft.Cc(addresses);
        return this;
    }

    /// <summary>Adds carbon-copy recipients to the message.</summary>
    /// <param name="mailboxAddresses">The carbon-copy recipient mailbox addresses.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailMessageBuilder Cc(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _draft.Cc(mailboxAddresses);
        return this;
    }

    /// <summary>Adds carbon-copy recipients to the message.</summary>
    /// <param name="mailboxAddresses">The carbon-copy recipient mailbox addresses.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailMessageBuilder Cc(IList<MailboxAddress> mailboxAddresses)
    {
        _draft.Cc(mailboxAddresses);
        return this;
    }

    /// <summary>Adds a blind-carbon-copy recipient to the message.</summary>
    /// <param name="address">The blind-carbon-copy recipient email address.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailMessageBuilder Bcc(string address)
    {
        _draft.Bcc(address);
        return this;
    }

    /// <summary>Adds a blind-carbon-copy recipient to the message.</summary>
    /// <param name="address">The blind-carbon-copy recipient email address.</param>
    /// <param name="name">The optional blind-carbon-copy recipient display name.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailMessageBuilder Bcc(string address, string? name)
    {
        _draft.Bcc(address, name);
        return this;
    }

    /// <summary>Adds a blind-carbon-copy recipient to the message.</summary>
    /// <param name="mailboxAddress">The blind-carbon-copy recipient mailbox address.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailMessageBuilder Bcc(MailboxAddress mailboxAddress)
    {
        _draft.Bcc(mailboxAddress);
        return this;
    }

    /// <summary>Adds blind-carbon-copy recipients to the message.</summary>
    /// <param name="addresses">The blind-carbon-copy recipient email addresses.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailMessageBuilder Bcc(IEnumerable<string> addresses)
    {
        _draft.Bcc(addresses);
        return this;
    }

    /// <summary>Adds blind-carbon-copy recipients to the message.</summary>
    /// <param name="mailboxAddresses">The blind-carbon-copy recipient mailbox addresses.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailMessageBuilder Bcc(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _draft.Bcc(mailboxAddresses);
        return this;
    }

    /// <summary>Adds blind-carbon-copy recipients to the message.</summary>
    /// <param name="mailboxAddresses">The blind-carbon-copy recipient mailbox addresses.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailMessageBuilder Bcc(IList<MailboxAddress> mailboxAddresses)
    {
        _draft.Bcc(mailboxAddresses);
        return this;
    }

    /// <summary>Sets the model that will be merged into the Postmark template for this message.</summary>
    /// <param name="templateModel">The template model.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailMessageBuilder WithModel(object templateModel)
    {
        _draft.WithModel(templateModel);
        return this;
    }

    /// <summary>Sets the model that will be merged into the Postmark template for this message.</summary>
    /// <param name="templateModel">The template model.</param>
    /// <param name="serializerOptions">The JSON serializer options used to snapshot the template model.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailMessageBuilder WithModel(object templateModel, JsonSerializerOptions serializerOptions)
    {
        _draft.WithModel(templateModel, serializerOptions);
        return this;
    }

    /// <summary>Adds a metadata entry that applies only to this message.</summary>
    /// <param name="name">The metadata name.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailMessageBuilder AddMetadata(string name, string value)
    {
        _draft.AddMetadata(name, value);
        return this;
    }

    /// <summary>Adds a metadata entry that applies only to this message.</summary>
    /// <param name="entry">The metadata entry to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailMessageBuilder AddMetadata(KeyValuePair<string, string> entry)
    {
        _draft.AddMetadata(entry);
        return this;
    }

    /// <summary>Adds metadata entries that apply only to this message.</summary>
    /// <param name="metadata">The metadata entries to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailMessageBuilder AddMetadata(IEnumerable<KeyValuePair<string, string>> metadata)
    {
        _draft.AddMetadata(metadata);
        return this;
    }

    /// <summary>Adds metadata entries that apply only to this message.</summary>
    /// <param name="metadata">The metadata entries to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailMessageBuilder AddMetadata(IDictionary<string, string> metadata)
    {
        _draft.AddMetadata(metadata);
        return this;
    }

    /// <summary>Adds a custom header that applies only to this message.</summary>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailMessageBuilder AddHeader(string name, string value)
    {
        _draft.AddHeader(name, value);
        return this;
    }

    /// <summary>Adds a custom header that applies only to this message.</summary>
    /// <param name="header">The header entry to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailMessageBuilder AddHeader(KeyValuePair<string, string> header)
    {
        _draft.AddHeader(header);
        return this;
    }

    /// <summary>Adds custom headers that apply only to this message.</summary>
    /// <param name="headers">The header entries to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailMessageBuilder AddHeader(IEnumerable<KeyValuePair<string, string>> headers)
    {
        _draft.AddHeader(headers);
        return this;
    }

    /// <summary>Adds custom headers that apply only to this message.</summary>
    /// <param name="headers">The header entries to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailMessageBuilder AddHeader(IDictionary<string, string> headers)
    {
        _draft.AddHeader(headers);
        return this;
    }

    /// <summary>Creates the configured <see cref="BulkEmailMessage" />.</summary>
    /// <returns>The configured bulk email message.</returns>
    public BulkEmailMessage Build()
    {
        return _draft.Build();
    }
}