using MimeKit;

namespace PostKit;

partial class EmailBuilder
{
    private List<MailboxAddress>? _replyTo;

    public EmailBuilder ReplyTo(string address)
    {
        var mailboxAddress = MailboxAddress.Parse(address);
        _replyTo ??= [];
        _replyTo.Add(mailboxAddress);
        return this;
    }

    public EmailBuilder ReplyTo(string name, string address)
    {
        var mailboxAddress = new MailboxAddress(name, address);
        _replyTo ??= [];
        _replyTo.Add(mailboxAddress);
        return this;
    }

    public EmailBuilder ReplyTo(MailboxAddress mailboxAddress)
    {
        _replyTo ??= [];
        _replyTo.Add(mailboxAddress);
        return this;
    }

    public EmailBuilder ReplyTo(params IEnumerable<string> addresses)
    {
        var mailboxAddresses = addresses.Select(MailboxAddress.Parse);
        _replyTo ??= [];
        _replyTo.AddRange(mailboxAddresses);
        return this;
    }

    public EmailBuilder ReplyTo(params IEnumerable<(string Name, string Address)> addresses)
    {
        var mailboxAddresses = addresses.Select(x => new MailboxAddress(x.Name, x.Address));
        _replyTo ??= [];
        _replyTo.AddRange(mailboxAddresses);
        return this;
    }

    public EmailBuilder ReplyTo(params IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _replyTo ??= [];
        _replyTo.AddRange(mailboxAddresses);
        return this;
    }
}
