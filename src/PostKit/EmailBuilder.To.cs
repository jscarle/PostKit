using MimeKit;
using PostKit.Common;

namespace PostKit;

partial class EmailBuilder : IEmailToBuilder
{
    private IList<MailboxAddress>? _to;

    public IEmailToBuilder To(string address)
    {
        _to.EnsureNotSet(nameof(Email.To));

        _to = address.ToAddressList();

        return this;
    }

    public IEmailToBuilder To(string name, string address)
    {
        _to.EnsureNotSet(nameof(Email.To));

        _to = (name, address).ToAddressList();

        return this;
    }

    public IEmailToBuilder To(MailboxAddress mailboxAddress)
    {
        _to.EnsureNotSet(nameof(Email.To));

        _to = mailboxAddress.ToAddressList();
        
        return this;
    }

    public IEmailToBuilder To(IEnumerable<string> addresses)
    {
        _to.EnsureNotSet(nameof(Email.To));

        _to = addresses.ToAddressList();

        return this;
    }

    public IEmailToBuilder To(IList<MailboxAddress> mailboxAddresses)
    {
        _to.EnsureNotSet(nameof(Email.To));

        _to = mailboxAddresses;

        return this;
    }
    
    public IEmailToBuilder AlsoTo(string address)
    {
        var mailboxAddresses = address.ToAddressList();

        _to!.AddRange(mailboxAddresses);

        return this;
    }

    public IEmailToBuilder AlsoTo(string name, string address)
    {
        var mailboxAddresses = (name, address).ToAddressList();

        _to!.AddRange(mailboxAddresses);

        return this;
    }

    public IEmailToBuilder AlsoTo(MailboxAddress mailboxAddress)
    {
        var mailboxAddresses = mailboxAddress.ToAddressList();

        _to!.AddRange(mailboxAddresses);
        
        return this;
    }

    public IEmailToBuilder AlsoTo(IEnumerable<string> addresses)
    {
        var mailboxAddresses = addresses.ToAddressList();

        _to!.AddRange(mailboxAddresses);

        return this;
    }

    public IEmailToBuilder AlsoTo(IList<MailboxAddress> mailboxAddresses)
    {
        _to!.AddRange(mailboxAddresses);

        return this;
    }
}
