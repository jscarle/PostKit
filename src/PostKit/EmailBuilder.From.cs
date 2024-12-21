using MimeKit;

namespace PostKit;

partial class EmailBuilder
{
    private MailboxAddress? _from;

    public EmailBuilder From(string address)
    {
        _from = MailboxAddress.Parse(address);
        return this;
    }

    public EmailBuilder From(string name, string address)
    {
        _from = new MailboxAddress(name, address);
        return this;
    }

    public EmailBuilder From(MailboxAddress mailboxAddress)
    {
        _from = mailboxAddress;
        return this;
    }
}