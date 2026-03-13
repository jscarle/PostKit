using MimeKit;
using PostKit.Postmark.Common;

namespace PostKit.Emails;

partial class EmailBuilder : IEmailToBuilder
{
    private IList<MailboxAddress>? _to;

    /// <inheritdoc/>
    public IEmailToBuilder To(string address)
    {
        _to.EnsureNotSet(nameof(Email.To));

        _to = address.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IEmailToBuilder To(string? name, string address)
    {
        _to.EnsureNotSet(nameof(Email.To));

        _to = (name, address).ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IEmailToBuilder To(MailboxAddress mailboxAddress)
    {
        _to.EnsureNotSet(nameof(Email.To));

        _to = mailboxAddress.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IEmailToBuilder To(IEnumerable<string> addresses)
    {
        _to.EnsureNotSet(nameof(Email.To));

        _to = addresses.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IEmailToBuilder To(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _to.EnsureNotSet(nameof(Email.To));

        _to = mailboxAddresses.ToList();

        return this;
    }

    /// <inheritdoc/>
    public IEmailToBuilder To(IList<MailboxAddress> mailboxAddresses)
    {
        _to.EnsureNotSet(nameof(Email.To));

        _to = mailboxAddresses.ToList();

        return this;
    }

    /// <inheritdoc/>
    public IEmailToBuilder AlsoTo(string address)
    {
        if (_to is null)
            throw new InvalidOperationException("To() must be called before AlsoTo().");

        var mailboxAddresses = address.ToAddressList();

        _to.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IEmailToBuilder AlsoTo(string? name, string address)
    {
        if (_to is null)
            throw new InvalidOperationException("To() must be called before AlsoTo().");

        var mailboxAddresses = (name, address).ToAddressList();

        _to.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IEmailToBuilder AlsoTo(MailboxAddress mailboxAddress)
    {
        if (_to is null)
            throw new InvalidOperationException("To() must be called before AlsoTo().");

        var mailboxAddresses = mailboxAddress.ToAddressList();

        _to.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IEmailToBuilder AlsoTo(IEnumerable<string> addresses)
    {
        if (_to is null)
            throw new InvalidOperationException("To() must be called before AlsoTo().");

        var mailboxAddresses = addresses.ToAddressList();

        _to.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IEmailToBuilder AlsoTo(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        if (_to is null)
            throw new InvalidOperationException("To() must be called before AlsoTo().");

        foreach (var mailboxAddress in mailboxAddresses)
            _to.Add(mailboxAddress);

        return this;
    }

    /// <inheritdoc/>
    public IEmailToBuilder AlsoTo(IList<MailboxAddress> mailboxAddresses)
    {
        if (_to is null)
            throw new InvalidOperationException("To() must be called before AlsoTo().");

        _to.AddRange(mailboxAddresses);

        return this;
    }
}
