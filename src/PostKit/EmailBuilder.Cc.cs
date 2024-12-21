using MimeKit;

namespace PostKit;

partial class EmailBuilder
{
    private List<MailboxAddress>? _cc;

    public EmailBuilder Cc(string address)
    {
        var mailboxAddress = MailboxAddress.Parse(address);
        _cc ??= [];
        _cc.Add(mailboxAddress);
        return this;
    }

    public EmailBuilder Cc(string name, string address)
    {
        var mailboxAddress = new MailboxAddress(name, address);
        _cc ??= [];
        _cc.Add(mailboxAddress);
        return this;
    }

    public EmailBuilder Cc(MailboxAddress mailboxAddress)
    {
        _cc ??= [];
        _cc.Add(mailboxAddress);
        return this;
    }

    public EmailBuilder Cc(params IEnumerable<string> addresses)
    {
        var mailboxAddresses = addresses.Select(MailboxAddress.Parse);
        _cc ??= [];
        _cc.AddRange(mailboxAddresses);
        return this;
    }

    public EmailBuilder Cc(params IEnumerable<(string Name, string Address)> addresses)
    {
        var mailboxAddresses = addresses.Select(x => new MailboxAddress(x.Name, x.Address));
        _cc ??= [];
        _cc.AddRange(mailboxAddresses);
        return this;
    }

    public EmailBuilder Cc(params IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _cc ??= [];
        _cc.AddRange(mailboxAddresses);
        return this;
    }
}
