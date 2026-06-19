using System.Text.Json;
using MimeKit;
using PostKit.Common;

namespace PostKit.Emails;

/// <summary>Provides a fluent interface for composing an <see cref="Email" /> from a Postmark template.</summary>
public sealed class TemplatedEmailBuilder
{
    private readonly DraftEmail _draft = new();

    internal TemplatedEmailBuilder(int templateId, bool? inlineCss = null)
    {
        _draft.SetTemplate(templateId, inlineCss);
    }

    internal TemplatedEmailBuilder(string templateAlias, bool? inlineCss = null)
    {
        _draft.SetTemplate(templateAlias, inlineCss);
    }

    /// <summary>Sets the sender address for the email.</summary>
    /// <param name="address">The sender's email address.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder From(string address)
    {
        _draft.From(address);
        return this;
    }

    /// <summary>Sets the sender address for the email.</summary>
    /// <param name="address">The sender's email address.</param>
    /// <param name="name">The optional sender display name.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder From(string address, string? name)
    {
        _draft.From(address, name);
        return this;
    }

    /// <summary>Sets the sender address for the email.</summary>
    /// <param name="mailboxAddress">The sender mailbox address.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder From(MailboxAddress mailboxAddress)
    {
        _draft.From(mailboxAddress);
        return this;
    }

    /// <summary>Adds a reply-to recipient to the email.</summary>
    /// <param name="address">The reply-to email address.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder ReplyTo(string address)
    {
        _draft.ReplyTo(address);
        return this;
    }

    /// <summary>Adds a reply-to recipient to the email.</summary>
    /// <param name="address">The reply-to email address.</param>
    /// <param name="name">The optional reply-to display name.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder ReplyTo(string address, string? name)
    {
        _draft.ReplyTo(address, name);
        return this;
    }

    /// <summary>Adds a reply-to recipient to the email.</summary>
    /// <param name="mailboxAddress">The reply-to mailbox address.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder ReplyTo(MailboxAddress mailboxAddress)
    {
        _draft.ReplyTo(mailboxAddress);
        return this;
    }

    /// <summary>Adds reply-to recipients to the email.</summary>
    /// <param name="addresses">The reply-to email addresses.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder ReplyTo(IEnumerable<string> addresses)
    {
        _draft.ReplyTo(addresses);
        return this;
    }

    /// <summary>Adds reply-to recipients to the email.</summary>
    /// <param name="mailboxAddresses">The reply-to mailbox addresses.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder ReplyTo(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _draft.ReplyTo(mailboxAddresses);
        return this;
    }

    /// <summary>Adds reply-to recipients to the email.</summary>
    /// <param name="mailboxAddresses">The reply-to mailbox addresses.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder ReplyTo(IList<MailboxAddress> mailboxAddresses)
    {
        _draft.ReplyTo(mailboxAddresses);
        return this;
    }

    /// <summary>Adds a primary recipient to the email.</summary>
    /// <param name="address">The recipient email address.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder To(string address)
    {
        _draft.To(address);
        return this;
    }

    /// <summary>Adds a primary recipient to the email.</summary>
    /// <param name="address">The recipient email address.</param>
    /// <param name="name">The optional recipient display name.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder To(string address, string? name)
    {
        _draft.To(address, name);
        return this;
    }

    /// <summary>Adds a primary recipient to the email.</summary>
    /// <param name="mailboxAddress">The recipient mailbox address.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder To(MailboxAddress mailboxAddress)
    {
        _draft.To(mailboxAddress);
        return this;
    }

    /// <summary>Adds primary recipients to the email.</summary>
    /// <param name="addresses">The recipient email addresses.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder To(IEnumerable<string> addresses)
    {
        _draft.To(addresses);
        return this;
    }

    /// <summary>Adds primary recipients to the email.</summary>
    /// <param name="mailboxAddresses">The recipient mailbox addresses.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder To(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _draft.To(mailboxAddresses);
        return this;
    }

    /// <summary>Adds primary recipients to the email.</summary>
    /// <param name="mailboxAddresses">The recipient mailbox addresses.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder To(IList<MailboxAddress> mailboxAddresses)
    {
        _draft.To(mailboxAddresses);
        return this;
    }

    /// <summary>Adds a carbon-copy recipient to the email.</summary>
    /// <param name="address">The carbon-copy recipient email address.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder Cc(string address)
    {
        _draft.Cc(address);
        return this;
    }

    /// <summary>Adds a carbon-copy recipient to the email.</summary>
    /// <param name="address">The carbon-copy recipient email address.</param>
    /// <param name="name">The optional carbon-copy recipient display name.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder Cc(string address, string? name)
    {
        _draft.Cc(address, name);
        return this;
    }

    /// <summary>Adds a carbon-copy recipient to the email.</summary>
    /// <param name="mailboxAddress">The carbon-copy recipient mailbox address.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder Cc(MailboxAddress mailboxAddress)
    {
        _draft.Cc(mailboxAddress);
        return this;
    }

    /// <summary>Adds carbon-copy recipients to the email.</summary>
    /// <param name="addresses">The carbon-copy recipient email addresses.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder Cc(IEnumerable<string> addresses)
    {
        _draft.Cc(addresses);
        return this;
    }

    /// <summary>Adds carbon-copy recipients to the email.</summary>
    /// <param name="mailboxAddresses">The carbon-copy recipient mailbox addresses.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder Cc(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _draft.Cc(mailboxAddresses);
        return this;
    }

    /// <summary>Adds carbon-copy recipients to the email.</summary>
    /// <param name="mailboxAddresses">The carbon-copy recipient mailbox addresses.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder Cc(IList<MailboxAddress> mailboxAddresses)
    {
        _draft.Cc(mailboxAddresses);
        return this;
    }

    /// <summary>Adds a blind-carbon-copy recipient to the email.</summary>
    /// <param name="address">The blind-carbon-copy recipient email address.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder Bcc(string address)
    {
        _draft.Bcc(address);
        return this;
    }

    /// <summary>Adds a blind-carbon-copy recipient to the email.</summary>
    /// <param name="address">The blind-carbon-copy recipient email address.</param>
    /// <param name="name">The optional blind-carbon-copy recipient display name.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder Bcc(string address, string? name)
    {
        _draft.Bcc(address, name);
        return this;
    }

    /// <summary>Adds a blind-carbon-copy recipient to the email.</summary>
    /// <param name="mailboxAddress">The blind-carbon-copy recipient mailbox address.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder Bcc(MailboxAddress mailboxAddress)
    {
        _draft.Bcc(mailboxAddress);
        return this;
    }

    /// <summary>Adds blind-carbon-copy recipients to the email.</summary>
    /// <param name="addresses">The blind-carbon-copy recipient email addresses.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder Bcc(IEnumerable<string> addresses)
    {
        _draft.Bcc(addresses);
        return this;
    }

    /// <summary>Adds blind-carbon-copy recipients to the email.</summary>
    /// <param name="mailboxAddresses">The blind-carbon-copy recipient mailbox addresses.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder Bcc(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _draft.Bcc(mailboxAddresses);
        return this;
    }

    /// <summary>Adds blind-carbon-copy recipients to the email.</summary>
    /// <param name="mailboxAddresses">The blind-carbon-copy recipient mailbox addresses.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder Bcc(IList<MailboxAddress> mailboxAddresses)
    {
        _draft.Bcc(mailboxAddresses);
        return this;
    }

    /// <summary>Sets the optional tag used to categorize the email in Postmark.</summary>
    /// <param name="tag">The tag value.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder WithTag(string tag)
    {
        _draft.WithTag(tag);
        return this;
    }

    /// <summary>Sets the model that will be merged into the Postmark template.</summary>
    /// <param name="templateModel">The template model.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder WithModel(object templateModel)
    {
        _draft.WithModel(templateModel);
        return this;
    }

    /// <summary>Sets the model that will be merged into the Postmark template.</summary>
    /// <param name="templateModel">The template model.</param>
    /// <param name="serializerOptions">The JSON serializer options used to snapshot the template model.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder WithModel(object templateModel, JsonSerializerOptions serializerOptions)
    {
        _draft.WithModel(templateModel, serializerOptions);
        return this;
    }

    /// <summary>Adds a custom header to the email.</summary>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder AddHeader(string name, string value)
    {
        _draft.AddHeader(name, value);
        return this;
    }

    /// <summary>Adds a custom header to the email.</summary>
    /// <param name="header">The header entry to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder AddHeader(KeyValuePair<string, string> header)
    {
        _draft.AddHeader(header);
        return this;
    }

    /// <summary>Adds custom headers to the email.</summary>
    /// <param name="headers">The header entries to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder AddHeader(IEnumerable<KeyValuePair<string, string>> headers)
    {
        _draft.AddHeader(headers);
        return this;
    }

    /// <summary>Adds custom headers to the email.</summary>
    /// <param name="headers">The header entries to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder AddHeader(IDictionary<string, string> headers)
    {
        _draft.AddHeader(headers);
        return this;
    }

    /// <summary>Adds a metadata entry to the email.</summary>
    /// <param name="name">The metadata name.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder AddMetadata(string name, string value)
    {
        _draft.AddMetadata(name, value);
        return this;
    }

    /// <summary>Adds a metadata entry to the email.</summary>
    /// <param name="entry">The metadata entry to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder AddMetadata(KeyValuePair<string, string> entry)
    {
        _draft.AddMetadata(entry);
        return this;
    }

    /// <summary>Adds metadata entries to the email.</summary>
    /// <param name="metadata">The metadata entries to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder AddMetadata(IEnumerable<KeyValuePair<string, string>> metadata)
    {
        _draft.AddMetadata(metadata);
        return this;
    }

    /// <summary>Adds metadata entries to the email.</summary>
    /// <param name="metadata">The metadata entries to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder AddMetadata(IDictionary<string, string> metadata)
    {
        _draft.AddMetadata(metadata);
        return this;
    }

    /// <summary>Adds an attachment to the email.</summary>
    /// <param name="attachment">The attachment to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder AddAttachment(Attachment attachment)
    {
        _draft.AddAttachment(attachment);
        return this;
    }

    /// <summary>Adds attachments to the email.</summary>
    /// <param name="attachments">The attachments to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder AddAttachment(IEnumerable<Attachment> attachments)
    {
        _draft.AddAttachment(attachments);
        return this;
    }

    /// <summary>Enables open tracking for the email.</summary>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder EnableOpenTracking()
    {
        _draft.EnableOpenTracking();
        return this;
    }

    /// <summary>Sets the link tracking mode for the email.</summary>
    /// <param name="linkTracking">The link tracking mode to use.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder UseLinkTracking(LinkTracking linkTracking = LinkTracking.HtmlAndText)
    {
        _draft.UseLinkTracking(linkTracking);
        return this;
    }

    /// <summary>Sets the message stream for the email.</summary>
    /// <param name="messageStream">The known Postmark message stream to use.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder UseMessageStream(MessageStream messageStream)
    {
        _draft.UseMessageStream(messageStream);
        return this;
    }

    /// <summary>Sets the message stream for the email.</summary>
    /// <param name="messageStreamId">The Postmark message stream ID to use.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedEmailBuilder UseMessageStream(string messageStreamId)
    {
        _draft.UseMessageStream(messageStreamId);
        return this;
    }

    /// <summary>Creates the configured <see cref="Email" />.</summary>
    /// <returns>The configured email.</returns>
    public Email Build()
    {
        return _draft.Build();
    }
}