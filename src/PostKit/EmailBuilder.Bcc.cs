using MimeKit;
using PostKit.Common;

namespace PostKit;

partial class EmailBuilder : IEmailBccBuilder
{
    private IList<MailboxAddress>? _bcc;

    /// <inheritdoc/>
    public IEmailBccBuilder Bcc(string address)
    {
        _bcc.EnsureNotSet(nameof(Email.Bcc));

        _bcc = address.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IEmailBccBuilder Bcc(string? name, string address)
    {
        _bcc.EnsureNotSet(nameof(Email.Bcc));

        _bcc = (name, address).ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IEmailBccBuilder Bcc(MailboxAddress mailboxAddress)
    {
        _bcc.EnsureNotSet(nameof(Email.Bcc));

        _bcc = mailboxAddress.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IEmailBccBuilder Bcc(IEnumerable<string> addresses)
    {
        _bcc.EnsureNotSet(nameof(Email.Bcc));

        _bcc = addresses.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IEmailBccBuilder Bcc(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _bcc.EnsureNotSet(nameof(Email.Bcc));

        _bcc = mailboxAddresses.ToList();

        return this;
    }

    /// <inheritdoc/>
    public IEmailBccBuilder Bcc(IList<MailboxAddress> mailboxAddresses)
    {
        _bcc.EnsureNotSet(nameof(Email.Bcc));

        _bcc = mailboxAddresses;

        return this;
    }

    /// <inheritdoc/>
    public IEmailBccBuilder AlsoBcc(string address)
    {
        if (_bcc is null)
            throw new InvalidOperationException("Bcc() must be called before AlsoBcc().");

        var mailboxAddresses = address.ToAddressList();

        _bcc.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IEmailBccBuilder AlsoBcc(string? name, string address)
    {
        if (_bcc is null)
            throw new InvalidOperationException("Bcc() must be called before AlsoBcc().");

        var mailboxAddresses = (name, address).ToAddressList();

        _bcc.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IEmailBccBuilder AlsoBcc(MailboxAddress mailboxAddress)
    {
        if (_bcc is null)
            throw new InvalidOperationException("Bcc() must be called before AlsoBcc().");

        var mailboxAddresses = mailboxAddress.ToAddressList();

        _bcc.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IEmailBccBuilder AlsoBcc(IEnumerable<string> addresses)
    {
        if (_bcc is null)
            throw new InvalidOperationException("Bcc() must be called before AlsoBcc().");

        var mailboxAddresses = addresses.ToAddressList();

        _bcc.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IEmailBccBuilder AlsoBcc(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        if (_bcc is null)
            throw new InvalidOperationException("Bcc() must be called before AlsoBcc().");

        foreach (var mailboxAddress in mailboxAddresses)
            _bcc.Add(mailboxAddress);

        return this;
    }

    /// <inheritdoc/>
    public IEmailBccBuilder AlsoBcc(IList<MailboxAddress> mailboxAddresses)
    {
        if (_bcc is null)
            throw new InvalidOperationException("Bcc() must be called before AlsoBcc().");

        _bcc.AddRange(mailboxAddresses);

        return this;
    }
}
