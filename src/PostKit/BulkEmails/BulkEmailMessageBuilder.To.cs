using MimeKit;
using PostKit.Postmark.Common;

namespace PostKit.BulkEmails;

partial class BulkEmailMessageBuilder : IBulkEmailMessageToBuilder
{
    private IList<MailboxAddress>? _to;

    /// <inheritdoc/>
    public IBulkEmailMessageToBuilder To(string address)
    {
        _to.EnsureNotSet(nameof(BulkEmailMessage.To));

        _to = address.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageToBuilder To(string? name, string address)
    {
        _to.EnsureNotSet(nameof(BulkEmailMessage.To));

        _to = (name, address).ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageToBuilder To(MailboxAddress mailboxAddress)
    {
        _to.EnsureNotSet(nameof(BulkEmailMessage.To));

        _to = mailboxAddress.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageToBuilder To(IEnumerable<string> addresses)
    {
        _to.EnsureNotSet(nameof(BulkEmailMessage.To));

        _to = addresses.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageToBuilder To(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _to.EnsureNotSet(nameof(BulkEmailMessage.To));

        _to = mailboxAddresses.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageToBuilder To(IList<MailboxAddress> mailboxAddresses)
    {
        _to.EnsureNotSet(nameof(BulkEmailMessage.To));

        _to = mailboxAddresses.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageToBuilder AlsoTo(string address)
    {
        if (_to is null)
            throw new InvalidOperationException("To() must be called before AlsoTo().");

        var mailboxAddresses = address.ToAddressList();

        _to.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageToBuilder AlsoTo(string? name, string address)
    {
        if (_to is null)
            throw new InvalidOperationException("To() must be called before AlsoTo().");

        var mailboxAddresses = (name, address).ToAddressList();

        _to.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageToBuilder AlsoTo(MailboxAddress mailboxAddress)
    {
        if (_to is null)
            throw new InvalidOperationException("To() must be called before AlsoTo().");

        var mailboxAddresses = mailboxAddress.ToAddressList();

        _to.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageToBuilder AlsoTo(IEnumerable<string> addresses)
    {
        if (_to is null)
            throw new InvalidOperationException("To() must be called before AlsoTo().");

        var mailboxAddresses = addresses.ToAddressList();

        _to.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageToBuilder AlsoTo(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        if (_to is null)
            throw new InvalidOperationException("To() must be called before AlsoTo().");

        _to.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageToBuilder AlsoTo(IList<MailboxAddress> mailboxAddresses)
    {
        if (_to is null)
            throw new InvalidOperationException("To() must be called before AlsoTo().");

        _to.AddRange(mailboxAddresses);

        return this;
    }
}
