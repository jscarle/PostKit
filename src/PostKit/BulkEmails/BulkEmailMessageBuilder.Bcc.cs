using MimeKit;
using PostKit.Postmark.Common;

namespace PostKit.BulkEmails;

partial class BulkEmailMessageBuilder : IBulkEmailMessageBccBuilder
{
    private IList<MailboxAddress>? _bcc;

    /// <inheritdoc/>
    public IBulkEmailMessageBccBuilder Bcc(string address)
    {
        _bcc.EnsureNotSet(nameof(BulkEmailMessage.Bcc));

        _bcc = address.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageBccBuilder Bcc(string? name, string address)
    {
        _bcc.EnsureNotSet(nameof(BulkEmailMessage.Bcc));

        _bcc = (name, address).ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageBccBuilder Bcc(MailboxAddress mailboxAddress)
    {
        _bcc.EnsureNotSet(nameof(BulkEmailMessage.Bcc));

        _bcc = mailboxAddress.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageBccBuilder Bcc(IEnumerable<string> addresses)
    {
        _bcc.EnsureNotSet(nameof(BulkEmailMessage.Bcc));

        _bcc = addresses.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageBccBuilder Bcc(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _bcc.EnsureNotSet(nameof(BulkEmailMessage.Bcc));

        _bcc = mailboxAddresses.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageBccBuilder Bcc(IList<MailboxAddress> mailboxAddresses)
    {
        _bcc.EnsureNotSet(nameof(BulkEmailMessage.Bcc));

        _bcc = mailboxAddresses.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageBccBuilder AlsoBcc(string address)
    {
        if (_bcc is null)
            throw new InvalidOperationException("Bcc() must be called before AlsoBcc().");

        var mailboxAddresses = address.ToAddressList();

        _bcc.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageBccBuilder AlsoBcc(string? name, string address)
    {
        if (_bcc is null)
            throw new InvalidOperationException("Bcc() must be called before AlsoBcc().");

        var mailboxAddresses = (name, address).ToAddressList();

        _bcc.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageBccBuilder AlsoBcc(MailboxAddress mailboxAddress)
    {
        if (_bcc is null)
            throw new InvalidOperationException("Bcc() must be called before AlsoBcc().");

        var mailboxAddresses = mailboxAddress.ToAddressList();

        _bcc.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageBccBuilder AlsoBcc(IEnumerable<string> addresses)
    {
        if (_bcc is null)
            throw new InvalidOperationException("Bcc() must be called before AlsoBcc().");

        var mailboxAddresses = addresses.ToAddressList();

        _bcc.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageBccBuilder AlsoBcc(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        if (_bcc is null)
            throw new InvalidOperationException("Bcc() must be called before AlsoBcc().");

        _bcc.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageBccBuilder AlsoBcc(IList<MailboxAddress> mailboxAddresses)
    {
        if (_bcc is null)
            throw new InvalidOperationException("Bcc() must be called before AlsoBcc().");

        _bcc.AddRange(mailboxAddresses);

        return this;
    }
}
