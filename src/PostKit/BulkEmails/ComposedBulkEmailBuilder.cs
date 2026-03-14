using MimeKit;
using PostKit.Common;

namespace PostKit.BulkEmails;

/// <summary>Provides a fluent interface for composing a <see cref="BulkEmail"/> without a Postmark template.</summary>
public sealed class ComposedBulkEmailBuilder
{
    private readonly DraftBulkEmail _draft = new();

    internal ComposedBulkEmailBuilder()
    {
    }

    public ComposedBulkEmailBuilder From(string address)
    {
        _draft.From(address);
        return this;
    }

    public ComposedBulkEmailBuilder From(string? name, string address)
    {
        _draft.From(name, address);
        return this;
    }

    public ComposedBulkEmailBuilder From(MailboxAddress mailboxAddress)
    {
        _draft.From(mailboxAddress);
        return this;
    }

    public ComposedBulkEmailBuilder ReplyTo(string address)
    {
        _draft.ReplyTo(address);
        return this;
    }

    public ComposedBulkEmailBuilder ReplyTo(string? name, string address)
    {
        _draft.ReplyTo(name, address);
        return this;
    }

    public ComposedBulkEmailBuilder ReplyTo(MailboxAddress mailboxAddress)
    {
        _draft.ReplyTo(mailboxAddress);
        return this;
    }

    public ComposedBulkEmailBuilder ReplyTo(IEnumerable<string> addresses)
    {
        _draft.ReplyTo(addresses);
        return this;
    }

    public ComposedBulkEmailBuilder ReplyTo(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _draft.ReplyTo(mailboxAddresses);
        return this;
    }

    public ComposedBulkEmailBuilder ReplyTo(IList<MailboxAddress> mailboxAddresses)
    {
        _draft.ReplyTo(mailboxAddresses);
        return this;
    }

    public ComposedBulkEmailBuilder Subject(string subject)
    {
        _draft.Subject(subject);
        return this;
    }

    public ComposedBulkEmailBuilder HtmlBody(string htmlBody)
    {
        _draft.HtmlBody(htmlBody);
        return this;
    }

    public ComposedBulkEmailBuilder TextBody(string textBody)
    {
        _draft.TextBody(textBody);
        return this;
    }

    public ComposedBulkEmailBuilder WithTag(string tag)
    {
        _draft.WithTag(tag);
        return this;
    }

    public ComposedBulkEmailBuilder AddHeader(string name, string value)
    {
        _draft.AddHeader(name, value);
        return this;
    }

    public ComposedBulkEmailBuilder AddHeader(KeyValuePair<string, string> header)
    {
        _draft.AddHeader(header);
        return this;
    }

    public ComposedBulkEmailBuilder AddHeader(IEnumerable<KeyValuePair<string, string>> headers)
    {
        _draft.AddHeader(headers);
        return this;
    }

    public ComposedBulkEmailBuilder AddHeader(IDictionary<string, string> headers)
    {
        _draft.AddHeader(headers);
        return this;
    }

    public ComposedBulkEmailBuilder AddMetadata(string name, string value)
    {
        _draft.AddMetadata(name, value);
        return this;
    }

    public ComposedBulkEmailBuilder AddMetadata(KeyValuePair<string, string> entry)
    {
        _draft.AddMetadata(entry);
        return this;
    }

    public ComposedBulkEmailBuilder AddMetadata(IEnumerable<KeyValuePair<string, string>> metadata)
    {
        _draft.AddMetadata(metadata);
        return this;
    }

    public ComposedBulkEmailBuilder AddMetadata(IDictionary<string, string> metadata)
    {
        _draft.AddMetadata(metadata);
        return this;
    }

    public ComposedBulkEmailBuilder EnableOpenTracking()
    {
        _draft.EnableOpenTracking();
        return this;
    }

    public ComposedBulkEmailBuilder UseLinkTracking(LinkTracking linkTracking = LinkTracking.HtmlAndText)
    {
        _draft.UseLinkTracking(linkTracking);
        return this;
    }

    public ComposedBulkEmailBuilder UseMessageStream(MessageStream messageStream)
    {
        _draft.UseMessageStream(messageStream);
        return this;
    }

    public ComposedBulkEmailBuilder UseMessageStream(string messageStreamId)
    {
        _draft.UseMessageStream(messageStreamId);
        return this;
    }

    public ComposedBulkEmailBuilder AddAttachment(Attachment attachment)
    {
        _draft.AddAttachment(attachment);
        return this;
    }

    public ComposedBulkEmailBuilder AddAttachment(IEnumerable<Attachment> attachments)
    {
        _draft.AddAttachment(attachments);
        return this;
    }

    public ComposedBulkEmailBuilder AddMessage(BulkEmailMessage message)
    {
        _draft.AddMessage(message);
        return this;
    }

    public ComposedBulkEmailBuilder AddMessage(IEnumerable<BulkEmailMessage> messages)
    {
        _draft.AddMessage(messages);
        return this;
    }

    public BulkEmail Build()
    {
        return _draft.Build();
    }
}
