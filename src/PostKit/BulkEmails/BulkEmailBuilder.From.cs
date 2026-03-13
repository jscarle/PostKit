using MimeKit;
using PostKit.Postmark.Common;

namespace PostKit.BulkEmails;

partial class BulkEmailBuilder
{
    private MailboxAddress? _from;

    /// <inheritdoc/>
    public IBulkEmailBuilder From(string address)
    {
        _from.EnsureNotSet(nameof(BulkEmail.From));

        var mailboxAddress = MailboxAddress.Parse(address);

        ValidateFrom(mailboxAddress, nameof(address));

        _from = mailboxAddress;

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailBuilder From(string? name, string address)
    {
        _from.EnsureNotSet(nameof(BulkEmail.From));

        var mailboxAddress = new MailboxAddress(name, address);

        ValidateFrom(mailboxAddress, nameof(address));

        _from = mailboxAddress;

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailBuilder From(MailboxAddress mailboxAddress)
    {
        _from.EnsureNotSet(nameof(BulkEmail.From));

        ValidateFrom(mailboxAddress, nameof(mailboxAddress));

        _from = mailboxAddress;

        return this;
    }

    private static void ValidateFrom(MailboxAddress mailboxAddress, string paramName)
    {
        var fromString = mailboxAddress.ToString(true);
        var length = fromString.AsSpan()
            .GetPostmarkCharacterCount();
        if (length > 255)
            throw new ArgumentException($"The {nameof(BulkEmail.From)} address cannot exceed 255 characters.", paramName);
    }
}
