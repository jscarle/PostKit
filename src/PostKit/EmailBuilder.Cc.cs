using MimeKit;
using PostKit.Common;
using PostKit.Validation;

namespace PostKit;

partial class EmailBuilder : IEmailCcBuilder
{
    private IList<MailboxAddress>? _cc;

    public IEmailCcBuilder Cc(string address)
    {
        _cc.EnsureNotSet(nameof(Email.Cc));

        _cc = address.ToAddressList();

        return this;
    }

    public IEmailCcBuilder Cc(string name, string address)
    {
        _cc.EnsureNotSet(nameof(Email.Cc));

        _cc = (name, address).ToAddressList();

        return this;
    }

    public IEmailCcBuilder Cc(MailboxAddress mailboxAddress)
    {
        _cc.EnsureNotSet(nameof(Email.Cc));

        _cc = mailboxAddress.ToAddressList();

        return this;
    }

    public IEmailCcBuilder Cc(IEnumerable<string> addresses)
    {
        _cc.EnsureNotSet(nameof(Email.Cc));

        _cc = addresses.ToAddressList();

        return this;
    }

    public IEmailCcBuilder Cc(IList<MailboxAddress> mailboxAddresses)
    {
        _cc.EnsureNotSet(nameof(Email.Cc));

        _cc = mailboxAddresses;

        return this;
    }
    
    public IEmailCcBuilder AlsoCc(string address)
    {
        var mailboxAddresses = address.ToAddressList();

        _cc!.AddRange(mailboxAddresses);

        return this;
    }

    public IEmailCcBuilder AlsoCc(string name, string address)
    {
        var mailboxAddresses = (name, address).ToAddressList();

        _cc!.AddRange(mailboxAddresses);

        return this;
    }

    public IEmailCcBuilder AlsoCc(MailboxAddress mailboxAddress)
    {
        var mailboxAddresses = mailboxAddress.ToAddressList();

        _cc!.AddRange(mailboxAddresses);
        
        return this;
    }

    public IEmailCcBuilder AlsoCc(IEnumerable<string> addresses)
    {
        var mailboxAddresses = addresses.ToAddressList();

        _cc!.AddRange(mailboxAddresses);

        return this;
    }

    public IEmailCcBuilder AlsoCc(IList<MailboxAddress> mailboxAddresses)
    {
        _cc!.AddRange(mailboxAddresses);

        return this;
    }
}
