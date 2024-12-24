using MimeKit;
using PostKit.Validation;

namespace PostKit;

partial class EmailBuilder
{
    private MailboxAddress? _from;

    public IEmailBuilder From(string address)
    {
        _from.EnsureNotSet(nameof(Email.From));

        var mailboxAddress = MailboxAddress.Parse(address);

        ValidateFrom(mailboxAddress, nameof(address));

        _from = mailboxAddress;

        return this;
    }

    public IEmailBuilder From(string name, string address)
    {
        _from.EnsureNotSet(nameof(Email.From));

        var mailboxAddress = new MailboxAddress(name, address);

        ValidateFrom(mailboxAddress, nameof(address));

        _from = mailboxAddress;

        return this;
    }

    public IEmailBuilder From(MailboxAddress mailboxAddress)
    {
        _from.EnsureNotSet(nameof(Email.From));

        ValidateFrom(mailboxAddress, nameof(mailboxAddress));

        _from = mailboxAddress;

        return this;
    }

    private static void ValidateFrom(MailboxAddress mailboxAddress, string paramName)
    {
        var fromString = mailboxAddress.ToString(true);
        var length = fromString.AsSpan()
            .GetUtf16Length();
        if (length > 255)
            throw new ArgumentException($"The {nameof(Email.From)} address cannot exceed 255 characters.", paramName);
    }
}
