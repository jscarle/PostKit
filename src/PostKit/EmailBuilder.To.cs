using MimeKit;

namespace PostKit;

partial class EmailBuilder
{
    private readonly List<MailboxAddress> _to = [];

    public EmailBuilder To(string address)
    {
        var mailboxAddress = MailboxAddress.Parse(address);
        _to.Add(mailboxAddress);
        return this;
    }

    public EmailBuilder To(string name, string address)
    {
        var mailboxAddress = new MailboxAddress(name, address);
        _to.Add(mailboxAddress);
        return this;
    }

    public EmailBuilder To(MailboxAddress mailboxAddress)
    {
        _to.Add(mailboxAddress);
        return this;
    }

    public EmailBuilder To(params IEnumerable<string> addresses)
    {
        var mailboxAddresses = addresses.Select(MailboxAddress.Parse);
        _to.AddRange(mailboxAddresses);
        return this;
    }

    public EmailBuilder To(params IEnumerable<(string Name, string Address)> addresses)
    {
        var mailboxAddresses = addresses.Select(x => new MailboxAddress(x.Name, x.Address));
        _to.AddRange(mailboxAddresses);
        return this;
    }

    public EmailBuilder To(params IEnumerable<MailboxAddress> mailboxAddresses)
    {
        _to.AddRange(mailboxAddresses);
        return this;
    }
}
