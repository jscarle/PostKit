using MimeKit;

namespace PostKit.BulkEmails;

/// <summary>Provides a fluent interface for composing a <see cref="BulkEmailMessage"/> without a template model.</summary>
public sealed class ComposedBulkEmailMessageBuilder
{
    private readonly DraftBulkEmailMessage _draft = new();

    internal ComposedBulkEmailMessageBuilder()
    {
    }

    public ComposedBulkEmailMessageBuilder To(string address)
    {
        _draft.To(address);
        return this;
    }

    public ComposedBulkEmailMessageBuilder To(string? name, string address)
    {
        _draft.To(name, address);
        return this;
    }

    public ComposedBulkEmailMessageBuilder To(MailboxAddress mailboxAddress)
    {
        _draft.To(mailboxAddress);
        return this;
    }

    public ComposedBulkEmailMessageBuilder To(IEnumerable<string> addresses)
    {
        _draft.To(addresses);
        return this;
    }

    public ComposedBulkEmailMessageBuilder To(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _draft.To(mailboxAddresses);
        return this;
    }

    public ComposedBulkEmailMessageBuilder To(IList<MailboxAddress> mailboxAddresses)
    {
        _draft.To(mailboxAddresses);
        return this;
    }

    public ComposedBulkEmailMessageBuilder Cc(string address)
    {
        _draft.Cc(address);
        return this;
    }

    public ComposedBulkEmailMessageBuilder Cc(string? name, string address)
    {
        _draft.Cc(name, address);
        return this;
    }

    public ComposedBulkEmailMessageBuilder Cc(MailboxAddress mailboxAddress)
    {
        _draft.Cc(mailboxAddress);
        return this;
    }

    public ComposedBulkEmailMessageBuilder Cc(IEnumerable<string> addresses)
    {
        _draft.Cc(addresses);
        return this;
    }

    public ComposedBulkEmailMessageBuilder Cc(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _draft.Cc(mailboxAddresses);
        return this;
    }

    public ComposedBulkEmailMessageBuilder Cc(IList<MailboxAddress> mailboxAddresses)
    {
        _draft.Cc(mailboxAddresses);
        return this;
    }

    public ComposedBulkEmailMessageBuilder Bcc(string address)
    {
        _draft.Bcc(address);
        return this;
    }

    public ComposedBulkEmailMessageBuilder Bcc(string? name, string address)
    {
        _draft.Bcc(name, address);
        return this;
    }

    public ComposedBulkEmailMessageBuilder Bcc(MailboxAddress mailboxAddress)
    {
        _draft.Bcc(mailboxAddress);
        return this;
    }

    public ComposedBulkEmailMessageBuilder Bcc(IEnumerable<string> addresses)
    {
        _draft.Bcc(addresses);
        return this;
    }

    public ComposedBulkEmailMessageBuilder Bcc(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _draft.Bcc(mailboxAddresses);
        return this;
    }

    public ComposedBulkEmailMessageBuilder Bcc(IList<MailboxAddress> mailboxAddresses)
    {
        _draft.Bcc(mailboxAddresses);
        return this;
    }

    public ComposedBulkEmailMessageBuilder AddMetadata(string name, string value)
    {
        _draft.AddMetadata(name, value);
        return this;
    }

    public ComposedBulkEmailMessageBuilder AddMetadata(KeyValuePair<string, string> entry)
    {
        _draft.AddMetadata(entry);
        return this;
    }

    public ComposedBulkEmailMessageBuilder AddMetadata(IEnumerable<KeyValuePair<string, string>> metadata)
    {
        _draft.AddMetadata(metadata);
        return this;
    }

    public ComposedBulkEmailMessageBuilder AddMetadata(IDictionary<string, string> metadata)
    {
        _draft.AddMetadata(metadata);
        return this;
    }

    public ComposedBulkEmailMessageBuilder AddHeader(string name, string value)
    {
        _draft.AddHeader(name, value);
        return this;
    }

    public ComposedBulkEmailMessageBuilder AddHeader(KeyValuePair<string, string> header)
    {
        _draft.AddHeader(header);
        return this;
    }

    public ComposedBulkEmailMessageBuilder AddHeader(IEnumerable<KeyValuePair<string, string>> headers)
    {
        _draft.AddHeader(headers);
        return this;
    }

    public ComposedBulkEmailMessageBuilder AddHeader(IDictionary<string, string> headers)
    {
        _draft.AddHeader(headers);
        return this;
    }

    public BulkEmailMessage Build()
    {
        return _draft.Build();
    }
}
