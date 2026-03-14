using MimeKit;
using PostKit.Common;

namespace PostKit.BulkEmails;

/// <summary>Provides a fluent interface for composing a <see cref="BulkEmail"/> from a Postmark template.</summary>
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

    public TemplatedBulkEmailBuilder From(string address)
    {
        _draft.From(address);
        return this;
    }

    public TemplatedBulkEmailBuilder From(string? name, string address)
    {
        _draft.From(name, address);
        return this;
    }

    public TemplatedBulkEmailBuilder From(MailboxAddress mailboxAddress)
    {
        _draft.From(mailboxAddress);
        return this;
    }

    public TemplatedBulkEmailBuilder ReplyTo(string address)
    {
        _draft.ReplyTo(address);
        return this;
    }

    public TemplatedBulkEmailBuilder ReplyTo(string? name, string address)
    {
        _draft.ReplyTo(name, address);
        return this;
    }

    public TemplatedBulkEmailBuilder ReplyTo(MailboxAddress mailboxAddress)
    {
        _draft.ReplyTo(mailboxAddress);
        return this;
    }

    public TemplatedBulkEmailBuilder ReplyTo(IEnumerable<string> addresses)
    {
        _draft.ReplyTo(addresses);
        return this;
    }

    public TemplatedBulkEmailBuilder ReplyTo(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _draft.ReplyTo(mailboxAddresses);
        return this;
    }

    public TemplatedBulkEmailBuilder ReplyTo(IList<MailboxAddress> mailboxAddresses)
    {
        _draft.ReplyTo(mailboxAddresses);
        return this;
    }

    public TemplatedBulkEmailBuilder WithTag(string tag)
    {
        _draft.WithTag(tag);
        return this;
    }

    public TemplatedBulkEmailBuilder AddHeader(string name, string value)
    {
        _draft.AddHeader(name, value);
        return this;
    }

    public TemplatedBulkEmailBuilder AddHeader(KeyValuePair<string, string> header)
    {
        _draft.AddHeader(header);
        return this;
    }

    public TemplatedBulkEmailBuilder AddHeader(IEnumerable<KeyValuePair<string, string>> headers)
    {
        _draft.AddHeader(headers);
        return this;
    }

    public TemplatedBulkEmailBuilder AddHeader(IDictionary<string, string> headers)
    {
        _draft.AddHeader(headers);
        return this;
    }

    public TemplatedBulkEmailBuilder AddMetadata(string name, string value)
    {
        _draft.AddMetadata(name, value);
        return this;
    }

    public TemplatedBulkEmailBuilder AddMetadata(KeyValuePair<string, string> entry)
    {
        _draft.AddMetadata(entry);
        return this;
    }

    public TemplatedBulkEmailBuilder AddMetadata(IEnumerable<KeyValuePair<string, string>> metadata)
    {
        _draft.AddMetadata(metadata);
        return this;
    }

    public TemplatedBulkEmailBuilder AddMetadata(IDictionary<string, string> metadata)
    {
        _draft.AddMetadata(metadata);
        return this;
    }

    public TemplatedBulkEmailBuilder EnableOpenTracking()
    {
        _draft.EnableOpenTracking();
        return this;
    }

    public TemplatedBulkEmailBuilder UseLinkTracking(LinkTracking linkTracking = LinkTracking.HtmlAndText)
    {
        _draft.UseLinkTracking(linkTracking);
        return this;
    }

    public TemplatedBulkEmailBuilder UseMessageStream(MessageStream messageStream)
    {
        _draft.UseMessageStream(messageStream);
        return this;
    }

    public TemplatedBulkEmailBuilder UseMessageStream(string messageStreamId)
    {
        _draft.UseMessageStream(messageStreamId);
        return this;
    }

    public TemplatedBulkEmailBuilder AddAttachment(Attachment attachment)
    {
        _draft.AddAttachment(attachment);
        return this;
    }

    public TemplatedBulkEmailBuilder AddAttachment(IEnumerable<Attachment> attachments)
    {
        _draft.AddAttachment(attachments);
        return this;
    }

    public TemplatedBulkEmailBuilder AddMessage(BulkEmailMessage message)
    {
        _draft.AddMessage(message);
        return this;
    }

    public TemplatedBulkEmailBuilder AddMessage(IEnumerable<BulkEmailMessage> messages)
    {
        _draft.AddMessage(messages);
        return this;
    }

    public BulkEmail Build()
    {
        return _draft.Build();
    }
}
