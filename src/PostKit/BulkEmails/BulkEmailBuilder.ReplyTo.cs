using MimeKit;
using PostKit.Postmark.Common;

namespace PostKit.BulkEmails;

partial class BulkEmailBuilder : IBulkEmailReplyToBuilder
{
    private IList<MailboxAddress>? _replyTo;

    /// <inheritdoc/>
    public IBulkEmailReplyToBuilder ReplyTo(string address)
    {
        _replyTo.EnsureNotSet(nameof(BulkEmail.ReplyTo));

        _replyTo = address.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailReplyToBuilder ReplyTo(string? name, string address)
    {
        _replyTo.EnsureNotSet(nameof(BulkEmail.ReplyTo));

        _replyTo = (name, address).ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailReplyToBuilder ReplyTo(MailboxAddress mailboxAddress)
    {
        _replyTo.EnsureNotSet(nameof(BulkEmail.ReplyTo));

        _replyTo = mailboxAddress.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailReplyToBuilder ReplyTo(IEnumerable<string> addresses)
    {
        _replyTo.EnsureNotSet(nameof(BulkEmail.ReplyTo));

        _replyTo = addresses.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailReplyToBuilder ReplyTo(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _replyTo.EnsureNotSet(nameof(BulkEmail.ReplyTo));

        _replyTo = mailboxAddresses.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailReplyToBuilder ReplyTo(IList<MailboxAddress> mailboxAddresses)
    {
        _replyTo.EnsureNotSet(nameof(BulkEmail.ReplyTo));

        _replyTo = mailboxAddresses.ToAddressList();

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailReplyToBuilder AlsoReplyTo(string address)
    {
        if (_replyTo is null)
            throw new InvalidOperationException("ReplyTo() must be called before AlsoReplyTo().");

        var mailboxAddresses = address.ToAddressList();

        _replyTo.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailReplyToBuilder AlsoReplyTo(string? name, string address)
    {
        if (_replyTo is null)
            throw new InvalidOperationException("ReplyTo() must be called before AlsoReplyTo().");

        var mailboxAddresses = (name, address).ToAddressList();

        _replyTo.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailReplyToBuilder AlsoReplyTo(MailboxAddress mailboxAddress)
    {
        if (_replyTo is null)
            throw new InvalidOperationException("ReplyTo() must be called before AlsoReplyTo().");

        var mailboxAddresses = mailboxAddress.ToAddressList();

        _replyTo.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailReplyToBuilder AlsoReplyTo(IEnumerable<string> addresses)
    {
        if (_replyTo is null)
            throw new InvalidOperationException("ReplyTo() must be called before AlsoReplyTo().");

        var mailboxAddresses = addresses.ToAddressList();

        _replyTo.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailReplyToBuilder AlsoReplyTo(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        if (_replyTo is null)
            throw new InvalidOperationException("ReplyTo() must be called before AlsoReplyTo().");

        _replyTo.AddRange(mailboxAddresses);

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailReplyToBuilder AlsoReplyTo(IList<MailboxAddress> mailboxAddresses)
    {
        if (_replyTo is null)
            throw new InvalidOperationException("ReplyTo() must be called before AlsoReplyTo().");

        _replyTo.AddRange(mailboxAddresses);

        return this;
    }
}
