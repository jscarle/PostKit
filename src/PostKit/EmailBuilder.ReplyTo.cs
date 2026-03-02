using MimeKit;
using PostKit.Common;

namespace PostKit;

partial class EmailBuilder : IEmailReplyToBuilder
{
    private IList<MailboxAddress>? _replyTo;

    /// <inheritdoc/>
    public IEmailReplyToBuilder ReplyTo(string address)
    {
        _replyTo.EnsureNotSet(nameof(Email.ReplyTo));

        _replyTo = address.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IEmailReplyToBuilder ReplyTo(string? name, string address)
    {
        _replyTo.EnsureNotSet(nameof(Email.ReplyTo));

        _replyTo = (name, address).ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IEmailReplyToBuilder ReplyTo(MailboxAddress mailboxAddress)
    {
        _replyTo.EnsureNotSet(nameof(Email.ReplyTo));

        _replyTo = mailboxAddress.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IEmailReplyToBuilder ReplyTo(IEnumerable<string> addresses)
    {
        _replyTo.EnsureNotSet(nameof(Email.ReplyTo));

        _replyTo = addresses.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IEmailReplyToBuilder ReplyTo(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _replyTo.EnsureNotSet(nameof(Email.ReplyTo));

        _replyTo = mailboxAddresses.ToList();

        return this;
    }

    /// <inheritdoc/>
    public IEmailReplyToBuilder ReplyTo(IList<MailboxAddress> mailboxAddresses)
    {
        _replyTo.EnsureNotSet(nameof(Email.ReplyTo));

        _replyTo = mailboxAddresses;

        return this;
    }

    /// <inheritdoc/>
    public IEmailReplyToBuilder AlsoReplyTo(string address)
    {
        if (_replyTo is null)
            throw new InvalidOperationException("ReplyTo() must be called before AlsoReplyTo().");

        var mailboxAddresses = address.ToAddressList();

        _replyTo.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IEmailReplyToBuilder AlsoReplyTo(string? name, string address)
    {
        if (_replyTo is null)
            throw new InvalidOperationException("ReplyTo() must be called before AlsoReplyTo().");

        var mailboxAddresses = (name, address).ToAddressList();

        _replyTo.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IEmailReplyToBuilder AlsoReplyTo(MailboxAddress mailboxAddress)
    {
        if (_replyTo is null)
            throw new InvalidOperationException("ReplyTo() must be called before AlsoReplyTo().");

        var mailboxAddresses = mailboxAddress.ToAddressList();

        _replyTo.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IEmailReplyToBuilder AlsoReplyTo(IEnumerable<string> addresses)
    {
        if (_replyTo is null)
            throw new InvalidOperationException("ReplyTo() must be called before AlsoReplyTo().");

        var mailboxAddresses = addresses.ToAddressList();

        _replyTo.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IEmailReplyToBuilder AlsoReplyTo(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        if (_replyTo is null)
            throw new InvalidOperationException("ReplyTo() must be called before AlsoReplyTo().");

        foreach (var mailboxAddress in mailboxAddresses)
            _replyTo.Add(mailboxAddress);

        return this;
    }

    /// <inheritdoc/>
    public IEmailReplyToBuilder AlsoReplyTo(IList<MailboxAddress> mailboxAddresses)
    {
        if (_replyTo is null)
            throw new InvalidOperationException("ReplyTo() must be called before AlsoReplyTo().");

        _replyTo.AddRange(mailboxAddresses);

        return this;
    }
}
