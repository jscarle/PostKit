using MimeKit;
using PostKit.Postmark.Common;

namespace PostKit.BulkEmails;

partial class BulkEmailMessageBuilder : IBulkEmailMessageCcBuilder
{
    private IList<MailboxAddress>? _cc;

    /// <inheritdoc/>
    public IBulkEmailMessageCcBuilder Cc(string address)
    {
        _cc.EnsureNotSet(nameof(BulkEmailMessage.Cc));

        _cc = address.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageCcBuilder Cc(string? name, string address)
    {
        _cc.EnsureNotSet(nameof(BulkEmailMessage.Cc));

        _cc = (name, address).ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageCcBuilder Cc(MailboxAddress mailboxAddress)
    {
        _cc.EnsureNotSet(nameof(BulkEmailMessage.Cc));

        _cc = mailboxAddress.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageCcBuilder Cc(IEnumerable<string> addresses)
    {
        _cc.EnsureNotSet(nameof(BulkEmailMessage.Cc));

        _cc = addresses.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageCcBuilder Cc(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _cc.EnsureNotSet(nameof(BulkEmailMessage.Cc));

        _cc = mailboxAddresses.ToList();

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageCcBuilder Cc(IList<MailboxAddress> mailboxAddresses)
    {
        _cc.EnsureNotSet(nameof(BulkEmailMessage.Cc));

        _cc = mailboxAddresses;

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageCcBuilder AlsoCc(string address)
    {
        if (_cc is null)
            throw new InvalidOperationException("Cc() must be called before AlsoCc().");

        var mailboxAddresses = address.ToAddressList();

        _cc.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageCcBuilder AlsoCc(string? name, string address)
    {
        if (_cc is null)
            throw new InvalidOperationException("Cc() must be called before AlsoCc().");

        var mailboxAddresses = (name, address).ToAddressList();

        _cc.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageCcBuilder AlsoCc(MailboxAddress mailboxAddress)
    {
        if (_cc is null)
            throw new InvalidOperationException("Cc() must be called before AlsoCc().");

        var mailboxAddresses = mailboxAddress.ToAddressList();

        _cc.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageCcBuilder AlsoCc(IEnumerable<string> addresses)
    {
        if (_cc is null)
            throw new InvalidOperationException("Cc() must be called before AlsoCc().");

        var mailboxAddresses = addresses.ToAddressList();

        _cc.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageCcBuilder AlsoCc(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        if (_cc is null)
            throw new InvalidOperationException("Cc() must be called before AlsoCc().");

        foreach (var mailboxAddress in mailboxAddresses)
            _cc.Add(mailboxAddress);

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageCcBuilder AlsoCc(IList<MailboxAddress> mailboxAddresses)
    {
        if (_cc is null)
            throw new InvalidOperationException("Cc() must be called before AlsoCc().");

        _cc.AddRange(mailboxAddresses);

        return this;
    }
}
