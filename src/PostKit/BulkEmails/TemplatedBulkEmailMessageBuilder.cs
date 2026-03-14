using System.Text.Json;
using MimeKit;

namespace PostKit.BulkEmails;

/// <summary>Provides a fluent interface for composing a <see cref="BulkEmailMessage"/> with a template model.</summary>
public sealed class TemplatedBulkEmailMessageBuilder
{
    private readonly DraftBulkEmailMessage _draft = new();

    internal TemplatedBulkEmailMessageBuilder()
    {
    }

    public TemplatedBulkEmailMessageBuilder To(string address)
    {
        _draft.To(address);
        return this;
    }

    public TemplatedBulkEmailMessageBuilder To(string? name, string address)
    {
        _draft.To(name, address);
        return this;
    }

    public TemplatedBulkEmailMessageBuilder To(MailboxAddress mailboxAddress)
    {
        _draft.To(mailboxAddress);
        return this;
    }

    public TemplatedBulkEmailMessageBuilder To(IEnumerable<string> addresses)
    {
        _draft.To(addresses);
        return this;
    }

    public TemplatedBulkEmailMessageBuilder To(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _draft.To(mailboxAddresses);
        return this;
    }

    public TemplatedBulkEmailMessageBuilder To(IList<MailboxAddress> mailboxAddresses)
    {
        _draft.To(mailboxAddresses);
        return this;
    }

    public TemplatedBulkEmailMessageBuilder Cc(string address)
    {
        _draft.Cc(address);
        return this;
    }

    public TemplatedBulkEmailMessageBuilder Cc(string? name, string address)
    {
        _draft.Cc(name, address);
        return this;
    }

    public TemplatedBulkEmailMessageBuilder Cc(MailboxAddress mailboxAddress)
    {
        _draft.Cc(mailboxAddress);
        return this;
    }

    public TemplatedBulkEmailMessageBuilder Cc(IEnumerable<string> addresses)
    {
        _draft.Cc(addresses);
        return this;
    }

    public TemplatedBulkEmailMessageBuilder Cc(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _draft.Cc(mailboxAddresses);
        return this;
    }

    public TemplatedBulkEmailMessageBuilder Cc(IList<MailboxAddress> mailboxAddresses)
    {
        _draft.Cc(mailboxAddresses);
        return this;
    }

    public TemplatedBulkEmailMessageBuilder Bcc(string address)
    {
        _draft.Bcc(address);
        return this;
    }

    public TemplatedBulkEmailMessageBuilder Bcc(string? name, string address)
    {
        _draft.Bcc(name, address);
        return this;
    }

    public TemplatedBulkEmailMessageBuilder Bcc(MailboxAddress mailboxAddress)
    {
        _draft.Bcc(mailboxAddress);
        return this;
    }

    public TemplatedBulkEmailMessageBuilder Bcc(IEnumerable<string> addresses)
    {
        _draft.Bcc(addresses);
        return this;
    }

    public TemplatedBulkEmailMessageBuilder Bcc(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _draft.Bcc(mailboxAddresses);
        return this;
    }

    public TemplatedBulkEmailMessageBuilder Bcc(IList<MailboxAddress> mailboxAddresses)
    {
        _draft.Bcc(mailboxAddresses);
        return this;
    }

    public TemplatedBulkEmailMessageBuilder WithModel(object templateModel)
    {
        _draft.WithModel(templateModel);
        return this;
    }

    public TemplatedBulkEmailMessageBuilder WithModel(object templateModel, JsonSerializerOptions serializerOptions)
    {
        _draft.WithModel(templateModel, serializerOptions);
        return this;
    }

    public TemplatedBulkEmailMessageBuilder AddMetadata(string name, string value)
    {
        _draft.AddMetadata(name, value);
        return this;
    }

    public TemplatedBulkEmailMessageBuilder AddMetadata(KeyValuePair<string, string> entry)
    {
        _draft.AddMetadata(entry);
        return this;
    }

    public TemplatedBulkEmailMessageBuilder AddMetadata(IEnumerable<KeyValuePair<string, string>> metadata)
    {
        _draft.AddMetadata(metadata);
        return this;
    }

    public TemplatedBulkEmailMessageBuilder AddMetadata(IDictionary<string, string> metadata)
    {
        _draft.AddMetadata(metadata);
        return this;
    }

    public TemplatedBulkEmailMessageBuilder AddHeader(string name, string value)
    {
        _draft.AddHeader(name, value);
        return this;
    }

    public TemplatedBulkEmailMessageBuilder AddHeader(KeyValuePair<string, string> header)
    {
        _draft.AddHeader(header);
        return this;
    }

    public TemplatedBulkEmailMessageBuilder AddHeader(IEnumerable<KeyValuePair<string, string>> headers)
    {
        _draft.AddHeader(headers);
        return this;
    }

    public TemplatedBulkEmailMessageBuilder AddHeader(IDictionary<string, string> headers)
    {
        _draft.AddHeader(headers);
        return this;
    }

    public BulkEmailMessage Build()
    {
        return _draft.Build();
    }
}
