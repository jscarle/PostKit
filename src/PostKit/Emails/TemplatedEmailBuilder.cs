using System.Text.Json;
using MimeKit;
using PostKit.Common;

namespace PostKit.Emails;

/// <summary>Provides a fluent interface for composing an <see cref="Email"/> from a Postmark template.</summary>
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

    public TemplatedEmailBuilder From(string address)
    {
        _draft.From(address);
        return this;
    }

    public TemplatedEmailBuilder From(string? name, string address)
    {
        _draft.From(name, address);
        return this;
    }

    public TemplatedEmailBuilder From(MailboxAddress mailboxAddress)
    {
        _draft.From(mailboxAddress);
        return this;
    }

    public TemplatedEmailBuilder ReplyTo(string address)
    {
        _draft.ReplyTo(address);
        return this;
    }

    public TemplatedEmailBuilder ReplyTo(string? name, string address)
    {
        _draft.ReplyTo(name, address);
        return this;
    }

    public TemplatedEmailBuilder ReplyTo(MailboxAddress mailboxAddress)
    {
        _draft.ReplyTo(mailboxAddress);
        return this;
    }

    public TemplatedEmailBuilder ReplyTo(IEnumerable<string> addresses)
    {
        _draft.ReplyTo(addresses);
        return this;
    }

    public TemplatedEmailBuilder ReplyTo(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _draft.ReplyTo(mailboxAddresses);
        return this;
    }

    public TemplatedEmailBuilder ReplyTo(IList<MailboxAddress> mailboxAddresses)
    {
        _draft.ReplyTo(mailboxAddresses);
        return this;
    }

    public TemplatedEmailBuilder To(string address)
    {
        _draft.To(address);
        return this;
    }

    public TemplatedEmailBuilder To(string? name, string address)
    {
        _draft.To(name, address);
        return this;
    }

    public TemplatedEmailBuilder To(MailboxAddress mailboxAddress)
    {
        _draft.To(mailboxAddress);
        return this;
    }

    public TemplatedEmailBuilder To(IEnumerable<string> addresses)
    {
        _draft.To(addresses);
        return this;
    }

    public TemplatedEmailBuilder To(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _draft.To(mailboxAddresses);
        return this;
    }

    public TemplatedEmailBuilder To(IList<MailboxAddress> mailboxAddresses)
    {
        _draft.To(mailboxAddresses);
        return this;
    }

    public TemplatedEmailBuilder Cc(string address)
    {
        _draft.Cc(address);
        return this;
    }

    public TemplatedEmailBuilder Cc(string? name, string address)
    {
        _draft.Cc(name, address);
        return this;
    }

    public TemplatedEmailBuilder Cc(MailboxAddress mailboxAddress)
    {
        _draft.Cc(mailboxAddress);
        return this;
    }

    public TemplatedEmailBuilder Cc(IEnumerable<string> addresses)
    {
        _draft.Cc(addresses);
        return this;
    }

    public TemplatedEmailBuilder Cc(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _draft.Cc(mailboxAddresses);
        return this;
    }

    public TemplatedEmailBuilder Cc(IList<MailboxAddress> mailboxAddresses)
    {
        _draft.Cc(mailboxAddresses);
        return this;
    }

    public TemplatedEmailBuilder Bcc(string address)
    {
        _draft.Bcc(address);
        return this;
    }

    public TemplatedEmailBuilder Bcc(string? name, string address)
    {
        _draft.Bcc(name, address);
        return this;
    }

    public TemplatedEmailBuilder Bcc(MailboxAddress mailboxAddress)
    {
        _draft.Bcc(mailboxAddress);
        return this;
    }

    public TemplatedEmailBuilder Bcc(IEnumerable<string> addresses)
    {
        _draft.Bcc(addresses);
        return this;
    }

    public TemplatedEmailBuilder Bcc(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _draft.Bcc(mailboxAddresses);
        return this;
    }

    public TemplatedEmailBuilder Bcc(IList<MailboxAddress> mailboxAddresses)
    {
        _draft.Bcc(mailboxAddresses);
        return this;
    }

    public TemplatedEmailBuilder WithTag(string tag)
    {
        _draft.WithTag(tag);
        return this;
    }

    public TemplatedEmailBuilder WithModel(object templateModel)
    {
        _draft.WithModel(templateModel);
        return this;
    }

    public TemplatedEmailBuilder WithModel(object templateModel, JsonSerializerOptions serializerOptions)
    {
        _draft.WithModel(templateModel, serializerOptions);
        return this;
    }

    public TemplatedEmailBuilder AddHeader(string name, string value)
    {
        _draft.AddHeader(name, value);
        return this;
    }

    public TemplatedEmailBuilder AddHeader(KeyValuePair<string, string> header)
    {
        _draft.AddHeader(header);
        return this;
    }

    public TemplatedEmailBuilder AddHeader(IEnumerable<KeyValuePair<string, string>> headers)
    {
        _draft.AddHeader(headers);
        return this;
    }

    public TemplatedEmailBuilder AddHeader(IDictionary<string, string> headers)
    {
        _draft.AddHeader(headers);
        return this;
    }

    public TemplatedEmailBuilder AddMetadata(string name, string value)
    {
        _draft.AddMetadata(name, value);
        return this;
    }

    public TemplatedEmailBuilder AddMetadata(KeyValuePair<string, string> entry)
    {
        _draft.AddMetadata(entry);
        return this;
    }

    public TemplatedEmailBuilder AddMetadata(IEnumerable<KeyValuePair<string, string>> metadata)
    {
        _draft.AddMetadata(metadata);
        return this;
    }

    public TemplatedEmailBuilder AddMetadata(IDictionary<string, string> metadata)
    {
        _draft.AddMetadata(metadata);
        return this;
    }

    public TemplatedEmailBuilder AddAttachment(Attachment attachment)
    {
        _draft.AddAttachment(attachment);
        return this;
    }

    public TemplatedEmailBuilder AddAttachment(IEnumerable<Attachment> attachments)
    {
        _draft.AddAttachment(attachments);
        return this;
    }

    public TemplatedEmailBuilder EnableOpenTracking()
    {
        _draft.EnableOpenTracking();
        return this;
    }

    public TemplatedEmailBuilder UseLinkTracking(LinkTracking linkTracking = LinkTracking.HtmlAndText)
    {
        _draft.UseLinkTracking(linkTracking);
        return this;
    }

    public TemplatedEmailBuilder UseMessageStream(MessageStream messageStream)
    {
        _draft.UseMessageStream(messageStream);
        return this;
    }

    public TemplatedEmailBuilder UseMessageStream(string messageStreamId)
    {
        _draft.UseMessageStream(messageStreamId);
        return this;
    }

    public Email Build()
    {
        return _draft.Build();
    }
}
