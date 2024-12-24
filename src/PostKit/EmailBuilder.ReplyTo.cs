using MimeKit;
using PostKit.Common;
using PostKit.Validation;

namespace PostKit;

partial class EmailBuilder : IEmailReplyToBuilder
{
    private IList<MailboxAddress>? _replyTo;

    public IEmailReplyToBuilder ReplyTo(string address)
    {
        _replyTo.EnsureNotSet(nameof(Email.ReplyTo));

        _replyTo = address.ToAddressList();

        return this;
    }

    public IEmailReplyToBuilder ReplyTo(string name, string address)
    {
        _replyTo.EnsureNotSet(nameof(Email.ReplyTo));

        _replyTo = (name, address).ToAddressList();

        return this;
    }

    public IEmailReplyToBuilder ReplyTo(MailboxAddress mailboxAddress)
    {
        _replyTo.EnsureNotSet(nameof(Email.ReplyTo));

        _replyTo = mailboxAddress.ToAddressList();

        return this;
    }

    public IEmailReplyToBuilder ReplyTo(IEnumerable<string> addresses)
    {
        _replyTo.EnsureNotSet(nameof(Email.ReplyTo));

        _replyTo = addresses.ToAddressList();

        return this;
    }

    public IEmailReplyToBuilder ReplyTo(IList<MailboxAddress> mailboxAddresses)
    {
        _replyTo.EnsureNotSet(nameof(Email.ReplyTo));

        _replyTo = mailboxAddresses;

        return this;
    }
    
    public IEmailReplyToBuilder AlsoReplyTo(string address)
    {
        var mailboxAddresses = address.ToAddressList();

        _replyTo!.AddRange(mailboxAddresses);

        return this;
    }

    public IEmailReplyToBuilder AlsoReplyTo(string name, string address)
    {
        var mailboxAddresses = (name, address).ToAddressList();

        _replyTo!.AddRange(mailboxAddresses);

        return this;
    }

    public IEmailReplyToBuilder AlsoReplyTo(MailboxAddress mailboxAddress)
    {
        var mailboxAddresses = mailboxAddress.ToAddressList();

        _replyTo!.AddRange(mailboxAddresses);
        
        return this;
    }

    public IEmailReplyToBuilder AlsoReplyTo(IEnumerable<string> addresses)
    {
        var mailboxAddresses = addresses.ToAddressList();

        _replyTo!.AddRange(mailboxAddresses);

        return this;
    }

    public IEmailReplyToBuilder AlsoReplyTo(IList<MailboxAddress> mailboxAddresses)
    {
        _replyTo!.AddRange(mailboxAddresses);

        return this;
    }
}
