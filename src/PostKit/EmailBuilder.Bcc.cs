using MimeKit;
using PostKit.Common;
using PostKit.Validation;

namespace PostKit;

partial class EmailBuilder : IEmailBccBuilder
{
    private IList<MailboxAddress>? _bcc;

    public IEmailBccBuilder Bcc(string address)
    {
        _bcc.EnsureNotSet(nameof(Email.Bcc));

        _bcc = address.ToAddressList();

        return this;
    }

    public IEmailBccBuilder Bcc(string name, string address)
    {
        _bcc.EnsureNotSet(nameof(Email.Bcc));

        _bcc = (name, address).ToAddressList();

        return this;
    }

    public IEmailBccBuilder Bcc(MailboxAddress mailboxAddress)
    {
        _bcc.EnsureNotSet(nameof(Email.Bcc));

        _bcc = mailboxAddress.ToAddressList();

        return this;
    }

    public IEmailBccBuilder Bcc(IEnumerable<string> addresses)
    {
        _bcc.EnsureNotSet(nameof(Email.Bcc));

        _bcc = addresses.ToAddressList();

        return this;
    }

    public IEmailBccBuilder Bcc(IList<MailboxAddress> mailboxAddresses)
    {
        _bcc.EnsureNotSet(nameof(Email.Bcc));

        _bcc = mailboxAddresses;

        return this;
    }
    
    public IEmailBccBuilder AlsoBcc(string address)
    {
        var mailboxAddresses = address.ToAddressList();

        _bcc!.AddRange(mailboxAddresses);

        return this;
    }

    public IEmailBccBuilder AlsoBcc(string name, string address)
    {
        var mailboxAddresses = (name, address).ToAddressList();

        _bcc!.AddRange(mailboxAddresses);

        return this;
    }

    public IEmailBccBuilder AlsoBcc(MailboxAddress mailboxAddress)
    {
        var mailboxAddresses = mailboxAddress.ToAddressList();

        _bcc!.AddRange(mailboxAddresses);
        
        return this;
    }

    public IEmailBccBuilder AlsoBcc(IEnumerable<string> addresses)
    {
        var mailboxAddresses = addresses.ToAddressList();

        _bcc!.AddRange(mailboxAddresses);

        return this;
    }

    public IEmailBccBuilder AlsoBcc(IList<MailboxAddress> mailboxAddresses)
    {
        _bcc!.AddRange(mailboxAddresses);

        return this;
    }
}
