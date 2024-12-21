using MimeKit;

namespace PostKit;

partial class EmailBuilder
{
    private List<MailboxAddress>? _bcc;

    public EmailBuilder Bcc(string address)
    {
        var mailboxAddress = MailboxAddress.Parse(address);
        _bcc ??= [];
        _bcc.Add(mailboxAddress);
        return this;
    }

    public EmailBuilder Bcc(string name, string address)
    {
        var mailboxAddress = new MailboxAddress(name, address);
        _bcc ??= [];
        _bcc.Add(mailboxAddress);
        return this;
    }

    public EmailBuilder Bcc(MailboxAddress mailboxAddress)
    {
        _bcc ??= [];
        _bcc.Add(mailboxAddress);
        return this;
    }

    public EmailBuilder Bcc(params IEnumerable<string> addresses)
    {
        var mailboxAddresses = addresses.Select(MailboxAddress.Parse);
        _bcc ??= [];
        _bcc.AddRange(mailboxAddresses);
        return this;
    }

    public EmailBuilder Bcc(params IEnumerable<(string Name, string Address)> addresses)
    {
        var mailboxAddresses = addresses.Select(x => new MailboxAddress(x.Name, x.Address));
        _bcc ??= [];
        _bcc.AddRange(mailboxAddresses);
        return this;
    }

    public EmailBuilder Bcc(params IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _bcc ??= [];
        _bcc.AddRange(mailboxAddresses);
        return this;
    }
}