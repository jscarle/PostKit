using MimeKit;
using PostKit.Common;

namespace PostKit.BulkEmails;

/// <summary>Provides a fluent interface for composing a <see cref="BulkEmail" /> without a Postmark template.</summary>
public sealed class ComposedBulkEmailBuilder
{
    private readonly DraftBulkEmail _draft = new();

    internal ComposedBulkEmailBuilder()
    {
    }

    /// <summary>Sets the sender address for the bulk email request.</summary>
    /// <param name="address">The sender email address.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder From(string address)
    {
        _draft.From(address);
        return this;
    }

    /// <summary>Sets the sender address for the bulk email request.</summary>
    /// <param name="address">The sender email address.</param>
    /// <param name="name">The optional sender display name.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder From(string address, string? name)
    {
        _draft.From(address, name);
        return this;
    }

    /// <summary>Sets the sender address for the bulk email request.</summary>
    /// <param name="mailboxAddress">The sender mailbox address.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder From(MailboxAddress mailboxAddress)
    {
        _draft.From(mailboxAddress);
        return this;
    }

    /// <summary>Adds a reply-to recipient to the bulk email request.</summary>
    /// <param name="address">The reply-to email address.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder ReplyTo(string address)
    {
        _draft.ReplyTo(address);
        return this;
    }

    /// <summary>Adds a reply-to recipient to the bulk email request.</summary>
    /// <param name="address">The reply-to email address.</param>
    /// <param name="name">The optional reply-to display name.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder ReplyTo(string address, string? name)
    {
        _draft.ReplyTo(address, name);
        return this;
    }

    /// <summary>Adds a reply-to recipient to the bulk email request.</summary>
    /// <param name="mailboxAddress">The reply-to mailbox address.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder ReplyTo(MailboxAddress mailboxAddress)
    {
        _draft.ReplyTo(mailboxAddress);
        return this;
    }

    /// <summary>Adds reply-to recipients to the bulk email request.</summary>
    /// <param name="addresses">The reply-to email addresses.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder ReplyTo(IEnumerable<string> addresses)
    {
        _draft.ReplyTo(addresses);
        return this;
    }

    /// <summary>Adds reply-to recipients to the bulk email request.</summary>
    /// <param name="mailboxAddresses">The reply-to mailbox addresses.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder ReplyTo(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _draft.ReplyTo(mailboxAddresses);
        return this;
    }

    /// <summary>Adds reply-to recipients to the bulk email request.</summary>
    /// <param name="mailboxAddresses">The reply-to mailbox addresses.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder ReplyTo(IList<MailboxAddress> mailboxAddresses)
    {
        _draft.ReplyTo(mailboxAddresses);
        return this;
    }

    /// <summary>Sets the subject shared by the bulk email request.</summary>
    /// <param name="subject">The shared subject line.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder Subject(string subject)
    {
        _draft.Subject(subject);
        return this;
    }

    /// <summary>Sets the HTML body shared by the bulk email request.</summary>
    /// <param name="htmlBody">The shared HTML body.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder HtmlBody(string htmlBody)
    {
        _draft.HtmlBody(htmlBody);
        return this;
    }

    /// <summary>Sets the plain-text body shared by the bulk email request.</summary>
    /// <param name="textBody">The shared plain-text body.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder TextBody(string textBody)
    {
        _draft.TextBody(textBody);
        return this;
    }

    /// <summary>Sets the optional tag used to categorize the bulk email request in Postmark.</summary>
    /// <param name="tag">The tag value.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder WithTag(string tag)
    {
        _draft.WithTag(tag);
        return this;
    }

    /// <summary>Adds a custom header shared by the bulk email request.</summary>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder AddHeader(string name, string value)
    {
        _draft.AddHeader(name, value);
        return this;
    }

    /// <summary>Adds a custom header shared by the bulk email request.</summary>
    /// <param name="header">The header entry to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder AddHeader(KeyValuePair<string, string> header)
    {
        _draft.AddHeader(header);
        return this;
    }

    /// <summary>Adds custom headers shared by the bulk email request.</summary>
    /// <param name="headers">The header entries to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder AddHeader(IEnumerable<KeyValuePair<string, string>> headers)
    {
        _draft.AddHeader(headers);
        return this;
    }

    /// <summary>Adds custom headers shared by the bulk email request.</summary>
    /// <param name="headers">The header entries to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder AddHeader(IDictionary<string, string> headers)
    {
        _draft.AddHeader(headers);
        return this;
    }

    /// <summary>Adds a metadata entry shared by the bulk email request.</summary>
    /// <param name="name">The metadata name.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder AddMetadata(string name, string value)
    {
        _draft.AddMetadata(name, value);
        return this;
    }

    /// <summary>Adds a metadata entry shared by the bulk email request.</summary>
    /// <param name="entry">The metadata entry to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder AddMetadata(KeyValuePair<string, string> entry)
    {
        _draft.AddMetadata(entry);
        return this;
    }

    /// <summary>Adds metadata entries shared by the bulk email request.</summary>
    /// <param name="metadata">The metadata entries to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder AddMetadata(IEnumerable<KeyValuePair<string, string>> metadata)
    {
        _draft.AddMetadata(metadata);
        return this;
    }

    /// <summary>Adds metadata entries shared by the bulk email request.</summary>
    /// <param name="metadata">The metadata entries to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder AddMetadata(IDictionary<string, string> metadata)
    {
        _draft.AddMetadata(metadata);
        return this;
    }

    /// <summary>Enables open tracking for the bulk email request.</summary>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder EnableOpenTracking()
    {
        _draft.EnableOpenTracking();
        return this;
    }

    /// <summary>Sets the link tracking mode for the bulk email request.</summary>
    /// <param name="linkTracking">The link tracking mode to use.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder UseLinkTracking(LinkTracking linkTracking = LinkTracking.HtmlAndText)
    {
        _draft.UseLinkTracking(linkTracking);
        return this;
    }

    /// <summary>Sets the message stream for the bulk email request.</summary>
    /// <param name="messageStream">The known Postmark message stream to use.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder UseMessageStream(MessageStream messageStream)
    {
        _draft.UseMessageStream(messageStream);
        return this;
    }

    /// <summary>Sets the message stream for the bulk email request.</summary>
    /// <param name="messageStreamId">The Postmark message stream ID to use.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder UseMessageStream(string messageStreamId)
    {
        _draft.UseMessageStream(messageStreamId);
        return this;
    }

    /// <summary>Adds an attachment shared by the bulk email request.</summary>
    /// <param name="attachment">The attachment to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder AddAttachment(Attachment attachment)
    {
        _draft.AddAttachment(attachment);
        return this;
    }

    /// <summary>Adds attachments shared by the bulk email request.</summary>
    /// <param name="attachments">The attachments to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder AddAttachment(IEnumerable<Attachment> attachments)
    {
        _draft.AddAttachment(attachments);
        return this;
    }

    /// <summary>Adds a recipient-specific message to the bulk email request.</summary>
    /// <param name="message">The recipient-specific message to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder AddMessage(BulkEmailMessage message)
    {
        _draft.AddMessage(message);
        return this;
    }

    /// <summary>Adds recipient-specific messages to the bulk email request.</summary>
    /// <param name="messages">The recipient-specific messages to add.</param>
    /// <returns>The same builder instance so calls can be chained.</returns>
    public ComposedBulkEmailBuilder AddMessage(IEnumerable<BulkEmailMessage> messages)
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
