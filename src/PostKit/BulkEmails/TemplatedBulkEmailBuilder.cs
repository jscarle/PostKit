using MimeKit;
using PostKit.Common;

namespace PostKit.BulkEmails;

/// <summary>Provides a fluent interface for composing a <see cref="BulkEmail" /> from a Postmark template.</summary>
public sealed class TemplatedBulkEmailBuilder
{
    private readonly DraftBulkEmail _draft = new();

    internal TemplatedBulkEmailBuilder(int templateId, bool? inlineCss = null)
    {
        _draft.SetTemplate(templateId, inlineCss);
    }

    internal TemplatedBulkEmailBuilder(string templateAlias, bool? inlineCss = null)
    {
        _draft.SetTemplate(templateAlias, inlineCss);
    }

    /// <summary>Sets the sender address for the bulk email request.</summary>
    /// <param name="address">The sender email address.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailBuilder From(string address)
    {
        _draft.From(address);
        return this;
    }

    /// <summary>Sets the sender address for the bulk email request.</summary>
    /// <param name="address">The sender email address.</param>
    /// <param name="name">The optional sender display name.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailBuilder From(string address, string? name)
    {
        _draft.From(address, name);
        return this;
    }

    /// <summary>Sets the sender address for the bulk email request.</summary>
    /// <param name="mailboxAddress">The sender mailbox address.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailBuilder From(MailboxAddress mailboxAddress)
    {
        _draft.From(mailboxAddress);
        return this;
    }

    /// <summary>Adds a reply-to recipient to the bulk email request.</summary>
    /// <param name="address">The reply-to email address.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailBuilder ReplyTo(string address)
    {
        _draft.ReplyTo(address);
        return this;
    }

    /// <summary>Adds a reply-to recipient to the bulk email request.</summary>
    /// <param name="address">The reply-to email address.</param>
    /// <param name="name">The optional reply-to display name.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailBuilder ReplyTo(string address, string? name)
    {
        _draft.ReplyTo(address, name);
        return this;
    }

    /// <summary>Adds a reply-to recipient to the bulk email request.</summary>
    /// <param name="mailboxAddress">The reply-to mailbox address.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailBuilder ReplyTo(MailboxAddress mailboxAddress)
    {
        _draft.ReplyTo(mailboxAddress);
        return this;
    }

    /// <summary>Adds reply-to recipients to the bulk email request.</summary>
    /// <param name="addresses">The reply-to email addresses.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailBuilder ReplyTo(IEnumerable<string> addresses)
    {
        _draft.ReplyTo(addresses);
        return this;
    }

    /// <summary>Adds reply-to recipients to the bulk email request.</summary>
    /// <param name="mailboxAddresses">The reply-to mailbox addresses.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailBuilder ReplyTo(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _draft.ReplyTo(mailboxAddresses);
        return this;
    }

    /// <summary>Adds reply-to recipients to the bulk email request.</summary>
    /// <param name="mailboxAddresses">The reply-to mailbox addresses.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailBuilder ReplyTo(IList<MailboxAddress> mailboxAddresses)
    {
        _draft.ReplyTo(mailboxAddresses);
        return this;
    }

    /// <summary>Sets the optional tag used to categorize the bulk email request in Postmark.</summary>
    /// <param name="tag">The tag value.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailBuilder WithTag(string tag)
    {
        _draft.WithTag(tag);
        return this;
    }

    /// <summary>Adds a custom header shared by the bulk email request.</summary>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailBuilder AddHeader(string name, string value)
    {
        _draft.AddHeader(name, value);
        return this;
    }

    /// <summary>Adds a custom header shared by the bulk email request.</summary>
    /// <param name="header">The header entry to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailBuilder AddHeader(KeyValuePair<string, string> header)
    {
        _draft.AddHeader(header);
        return this;
    }

    /// <summary>Adds custom headers shared by the bulk email request.</summary>
    /// <param name="headers">The header entries to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailBuilder AddHeader(IEnumerable<KeyValuePair<string, string>> headers)
    {
        _draft.AddHeader(headers);
        return this;
    }

    /// <summary>Adds custom headers shared by the bulk email request.</summary>
    /// <param name="headers">The header entries to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailBuilder AddHeader(IDictionary<string, string> headers)
    {
        _draft.AddHeader(headers);
        return this;
    }

    /// <summary>Adds a metadata entry shared by the bulk email request.</summary>
    /// <param name="name">The metadata name.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailBuilder AddMetadata(string name, string value)
    {
        _draft.AddMetadata(name, value);
        return this;
    }

    /// <summary>Adds a metadata entry shared by the bulk email request.</summary>
    /// <param name="entry">The metadata entry to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailBuilder AddMetadata(KeyValuePair<string, string> entry)
    {
        _draft.AddMetadata(entry);
        return this;
    }

    /// <summary>Adds metadata entries shared by the bulk email request.</summary>
    /// <param name="metadata">The metadata entries to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailBuilder AddMetadata(IEnumerable<KeyValuePair<string, string>> metadata)
    {
        _draft.AddMetadata(metadata);
        return this;
    }

    /// <summary>Adds metadata entries shared by the bulk email request.</summary>
    /// <param name="metadata">The metadata entries to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailBuilder AddMetadata(IDictionary<string, string> metadata)
    {
        _draft.AddMetadata(metadata);
        return this;
    }

    /// <summary>Enables open tracking for the bulk email request.</summary>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailBuilder EnableOpenTracking()
    {
        _draft.EnableOpenTracking();
        return this;
    }

    /// <summary>Sets the link tracking mode for the bulk email request.</summary>
    /// <param name="linkTracking">The link tracking mode to use.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailBuilder UseLinkTracking(LinkTracking linkTracking = LinkTracking.HtmlAndText)
    {
        _draft.UseLinkTracking(linkTracking);
        return this;
    }

    /// <summary>Sets the message stream for the bulk email request.</summary>
    /// <param name="messageStream">The known Postmark message stream to use.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailBuilder UseMessageStream(MessageStream messageStream)
    {
        _draft.UseMessageStream(messageStream);
        return this;
    }

    /// <summary>Sets the message stream for the bulk email request.</summary>
    /// <param name="messageStreamId">The Postmark message stream ID to use.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailBuilder UseMessageStream(string messageStreamId)
    {
        _draft.UseMessageStream(messageStreamId);
        return this;
    }

    /// <summary>Adds an attachment shared by the bulk email request.</summary>
    /// <param name="attachment">The attachment to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailBuilder AddAttachment(Attachment attachment)
    {
        _draft.AddAttachment(attachment);
        return this;
    }

    /// <summary>Adds attachments shared by the bulk email request.</summary>
    /// <param name="attachments">The attachments to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailBuilder AddAttachment(IEnumerable<Attachment> attachments)
    {
        _draft.AddAttachment(attachments);
        return this;
    }

    /// <summary>Adds a recipient-specific message to the bulk email request.</summary>
    /// <param name="message">The recipient-specific message to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailBuilder AddMessage(BulkEmailMessage message)
    {
        _draft.AddMessage(message);
        return this;
    }

    /// <summary>Adds recipient-specific messages to the bulk email request.</summary>
    /// <param name="messages">The recipient-specific messages to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public TemplatedBulkEmailBuilder AddMessage(IEnumerable<BulkEmailMessage> messages)
    {
        _draft.AddMessage(messages);
        return this;
    }

    /// <summary>Creates the configured <see cref="BulkEmail" />.</summary>
    /// <returns>The configured bulk email request.</returns>
    public BulkEmail Build()
    {
        return _draft.Build();
    }
}
