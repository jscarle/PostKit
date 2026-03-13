using MimeKit;
using PostKit.Postmark.Common;

namespace PostKit.Emails;

partial class EmailBuilder : IEmailCcBuilder
{
    private IList<MailboxAddress>? _cc;

    /// <inheritdoc/>
    public IEmailCcBuilder Cc(string address)
    {
        _cc.EnsureNotSet(nameof(Email.Cc));

        _cc = address.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IEmailCcBuilder Cc(string? name, string address)
    {
        _cc.EnsureNotSet(nameof(Email.Cc));

        _cc = (name, address).ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IEmailCcBuilder Cc(MailboxAddress mailboxAddress)
    {
        _cc.EnsureNotSet(nameof(Email.Cc));

        _cc = mailboxAddress.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IEmailCcBuilder Cc(IEnumerable<string> addresses)
    {
        _cc.EnsureNotSet(nameof(Email.Cc));

        _cc = addresses.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IEmailCcBuilder Cc(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _cc.EnsureNotSet(nameof(Email.Cc));

        _cc = mailboxAddresses.ToList();

        return this;
    }

    /// <inheritdoc/>
    public IEmailCcBuilder Cc(IList<MailboxAddress> mailboxAddresses)
    {
        _cc.EnsureNotSet(nameof(Email.Cc));

        _cc = mailboxAddresses.ToList();

        return this;
    }

    /// <inheritdoc/>
    public IEmailCcBuilder AlsoCc(string address)
    {
        if (_cc is null)
            throw new InvalidOperationException("Cc() must be called before AlsoCc().");

        var mailboxAddresses = address.ToAddressList();

        _cc.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IEmailCcBuilder AlsoCc(string? name, string address)
    {
        if (_cc is null)
            throw new InvalidOperationException("Cc() must be called before AlsoCc().");

        var mailboxAddresses = (name, address).ToAddressList();

        _cc.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IEmailCcBuilder AlsoCc(MailboxAddress mailboxAddress)
    {
        if (_cc is null)
            throw new InvalidOperationException("Cc() must be called before AlsoCc().");

        var mailboxAddresses = mailboxAddress.ToAddressList();

        _cc.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IEmailCcBuilder AlsoCc(IEnumerable<string> addresses)
    {
        if (_cc is null)
            throw new InvalidOperationException("Cc() must be called before AlsoCc().");

        var mailboxAddresses = addresses.ToAddressList();

        _cc.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IEmailCcBuilder AlsoCc(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        if (_cc is null)
            throw new InvalidOperationException("Cc() must be called before AlsoCc().");

        foreach (var mailboxAddress in mailboxAddresses)
            _cc.Add(mailboxAddress);

        return this;
    }

    /// <inheritdoc/>
    public IEmailCcBuilder AlsoCc(IList<MailboxAddress> mailboxAddresses)
    {
        if (_cc is null)
            throw new InvalidOperationException("Cc() must be called before AlsoCc().");

        _cc.AddRange(mailboxAddresses);

        return this;
    }
}
