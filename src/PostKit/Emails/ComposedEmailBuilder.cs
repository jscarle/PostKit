using MimeKit;
using PostKit.Common;

namespace PostKit.Emails;

/// <summary>Provides a fluent interface for composing an <see cref="Email"/> without a Postmark template.</summary>
public sealed class ComposedEmailBuilder
{
    private readonly DraftEmail _draft = new();

    internal ComposedEmailBuilder()
    {
    }

    public ComposedEmailBuilder From(string address)
    {
        _draft.From(address);
        return this;
    }

    public ComposedEmailBuilder From(string? name, string address)
    {
        _draft.From(name, address);
        return this;
    }

    public ComposedEmailBuilder From(MailboxAddress mailboxAddress)
    {
        _draft.From(mailboxAddress);
        return this;
    }

    public ComposedEmailBuilder ReplyTo(string address)
    {
        _draft.ReplyTo(address);
        return this;
    }

    public ComposedEmailBuilder ReplyTo(string? name, string address)
    {
        _draft.ReplyTo(name, address);
        return this;
    }

    public ComposedEmailBuilder ReplyTo(MailboxAddress mailboxAddress)
    {
        _draft.ReplyTo(mailboxAddress);
        return this;
    }

    public ComposedEmailBuilder ReplyTo(IEnumerable<string> addresses)
    {
        _draft.ReplyTo(addresses);
        return this;
    }

    public ComposedEmailBuilder ReplyTo(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _draft.ReplyTo(mailboxAddresses);
        return this;
    }

    public ComposedEmailBuilder ReplyTo(IList<MailboxAddress> mailboxAddresses)
    {
        _draft.ReplyTo(mailboxAddresses);
        return this;
    }

    public ComposedEmailBuilder To(string address)
    {
        _draft.To(address);
        return this;
    }

    public ComposedEmailBuilder To(string? name, string address)
    {
        _draft.To(name, address);
        return this;
    }

    public ComposedEmailBuilder To(MailboxAddress mailboxAddress)
    {
        _draft.To(mailboxAddress);
        return this;
    }

    public ComposedEmailBuilder To(IEnumerable<string> addresses)
    {
        _draft.To(addresses);
        return this;
    }

    public ComposedEmailBuilder To(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _draft.To(mailboxAddresses);
        return this;
    }

    public ComposedEmailBuilder To(IList<MailboxAddress> mailboxAddresses)
    {
        _draft.To(mailboxAddresses);
        return this;
    }

    public ComposedEmailBuilder Cc(string address)
    {
        _draft.Cc(address);
        return this;
    }

    public ComposedEmailBuilder Cc(string? name, string address)
    {
        _draft.Cc(name, address);
        return this;
    }

    public ComposedEmailBuilder Cc(MailboxAddress mailboxAddress)
    {
        _draft.Cc(mailboxAddress);
        return this;
    }

    public ComposedEmailBuilder Cc(IEnumerable<string> addresses)
    {
        _draft.Cc(addresses);
        return this;
    }

    public ComposedEmailBuilder Cc(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _draft.Cc(mailboxAddresses);
        return this;
    }

    public ComposedEmailBuilder Cc(IList<MailboxAddress> mailboxAddresses)
    {
        _draft.Cc(mailboxAddresses);
        return this;
    }

    public ComposedEmailBuilder Bcc(string address)
    {
        _draft.Bcc(address);
        return this;
    }

    public ComposedEmailBuilder Bcc(string? name, string address)
    {
        _draft.Bcc(name, address);
        return this;
    }

    public ComposedEmailBuilder Bcc(MailboxAddress mailboxAddress)
    {
        _draft.Bcc(mailboxAddress);
        return this;
    }

    public ComposedEmailBuilder Bcc(IEnumerable<string> addresses)
    {
        _draft.Bcc(addresses);
        return this;
    }

    public ComposedEmailBuilder Bcc(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _draft.Bcc(mailboxAddresses);
        return this;
    }

    public ComposedEmailBuilder Bcc(IList<MailboxAddress> mailboxAddresses)
    {
        _draft.Bcc(mailboxAddresses);
        return this;
    }

    public ComposedEmailBuilder Subject(string subject)
    {
        _draft.Subject(subject);
        return this;
    }

    public ComposedEmailBuilder HtmlBody(string html)
    {
        _draft.HtmlBody(html);
        return this;
    }

    public ComposedEmailBuilder TextBody(string text)
    {
        _draft.TextBody(text);
        return this;
    }

    public ComposedEmailBuilder WithTag(string tag)
    {
        _draft.WithTag(tag);
        return this;
    }

    public ComposedEmailBuilder AddHeader(string name, string value)
    {
        _draft.AddHeader(name, value);
        return this;
    }

    public ComposedEmailBuilder AddHeader(KeyValuePair<string, string> header)
    {
        _draft.AddHeader(header);
        return this;
    }

    public ComposedEmailBuilder AddHeader(IEnumerable<KeyValuePair<string, string>> headers)
    {
        _draft.AddHeader(headers);
        return this;
    }

    public ComposedEmailBuilder AddHeader(IDictionary<string, string> headers)
    {
        _draft.AddHeader(headers);
        return this;
    }

    public ComposedEmailBuilder AddMetadata(string name, string value)
    {
        _draft.AddMetadata(name, value);
        return this;
    }

    public ComposedEmailBuilder AddMetadata(KeyValuePair<string, string> entry)
    {
        _draft.AddMetadata(entry);
        return this;
    }

    public ComposedEmailBuilder AddMetadata(IEnumerable<KeyValuePair<string, string>> metadata)
    {
        _draft.AddMetadata(metadata);
        return this;
    }

    public ComposedEmailBuilder AddMetadata(IDictionary<string, string> metadata)
    {
        _draft.AddMetadata(metadata);
        return this;
    }

    public ComposedEmailBuilder AddAttachment(Attachment attachment)
    {
        _draft.AddAttachment(attachment);
        return this;
    }

    public ComposedEmailBuilder AddAttachment(IEnumerable<Attachment> attachments)
    {
        _draft.AddAttachment(attachments);
        return this;
    }

    public ComposedEmailBuilder EnableOpenTracking()
    {
        _draft.EnableOpenTracking();
        return this;
    }

    public ComposedEmailBuilder UseLinkTracking(LinkTracking linkTracking = LinkTracking.HtmlAndText)
    {
        _draft.UseLinkTracking(linkTracking);
        return this;
    }

    public ComposedEmailBuilder UseMessageStream(MessageStream messageStream)
    {
        _draft.UseMessageStream(messageStream);
        return this;
    }

    public ComposedEmailBuilder UseMessageStream(string messageStreamId)
    {
        _draft.UseMessageStream(messageStreamId);
        return this;
    }

    public Email Build()
    {
        return _draft.Build();
    }
}
