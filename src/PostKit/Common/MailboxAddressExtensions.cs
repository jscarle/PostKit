using MimeKit;

namespace PostKit.Common;

internal static class MailboxAddressExtensions
{
    private const string AddressFormatHint = "Use a valid mailbox address such as 'recipient@example.com'.";

    public static MailboxAddress ToMailboxAddress(this string address, string paramName = "address")
    {
        if (address is null)
            throw new ArgumentNullException(paramName, "The email address cannot be null.");

        try
        {
            var mailboxAddress = MailboxAddress.Parse(address);
            EnsureCompleteEmailAddress(mailboxAddress, paramName);
            return mailboxAddress;
        }
        catch (Exception ex) when (IsAddressParseException(ex))
        {
            throw new ArgumentException($"The email address is invalid. {AddressFormatHint}", paramName, ex);
        }
    }

    public static MailboxAddress ToMailboxAddress(this (string Address, string? Name) addressAndName, string paramName = "address")
    {
        if (addressAndName.Address is null)
            throw new ArgumentNullException(paramName, "The email address cannot be null.");

        try
        {
            var mailboxAddress = new MailboxAddress(addressAndName.Name, addressAndName.Address);
            EnsureCompleteEmailAddress(mailboxAddress, paramName);
            return mailboxAddress;
        }
        catch (Exception ex) when (IsAddressParseException(ex))
        {
            throw new ArgumentException($"The email address is invalid. {AddressFormatHint}", paramName, ex);
        }
    }

    public static IList<MailboxAddress> ToAddressList(this string address, string paramName = "address")
    {
        var mailboxAddress = address.ToMailboxAddress(paramName);
        return [mailboxAddress];
    }

    public static IList<MailboxAddress> ToAddressList(this (string Address, string? Name) addressAndName, string paramName = "address")
    {
        var mailboxAddress = addressAndName.ToMailboxAddress(paramName);
        return [mailboxAddress];
    }

    public static IList<MailboxAddress> ToAddressList(this MailboxAddress mailboxAddress)
    {
        if (mailboxAddress is null)
            throw new ArgumentNullException(nameof(mailboxAddress), "The email address cannot be null.");

        return [mailboxAddress.Snapshot()];
    }

    public static IList<MailboxAddress> ToAddressList(this IEnumerable<string> addresses, string paramName = "addresses")
    {
        if (addresses is null)
            throw new ArgumentNullException(paramName, "The email address collection cannot be null.");

        var list = new List<MailboxAddress>();
        var index = 0;
        foreach (var address in addresses)
        {
            if (address is null)
                throw new ArgumentNullException(paramName, $"The email address at index {index} cannot be null.");

            try
            {
                var mailboxAddress = MailboxAddress.Parse(address);
                EnsureCompleteEmailAddress(mailboxAddress, paramName, index);
                list.Add(mailboxAddress);
            }
            catch (Exception ex) when (IsAddressParseException(ex))
            {
                throw new ArgumentException($"The email address at index {index} is invalid. {AddressFormatHint}", paramName, ex);
            }

            index++;
        }

        return list;
    }

    public static IList<MailboxAddress> ToAddressList(this IEnumerable<MailboxAddress> mailboxAddresses, string paramName = "mailboxAddresses")
    {
        if (mailboxAddresses is null)
            throw new ArgumentNullException(paramName, "The email address collection cannot be null.");

        var list = new List<MailboxAddress>();
        var index = 0;
        foreach (var mailboxAddress in mailboxAddresses)
        {
            if (mailboxAddress is null)
                throw new ArgumentNullException(paramName, $"The email address at index {index} cannot be null.");

            list.Add(mailboxAddress.Snapshot());
            index++;
        }

        return list;
    }

    public static void AddRange(this IList<MailboxAddress> mailboxAddressesList, IEnumerable<MailboxAddress> mailboxAddresses, string paramName = "mailboxAddresses")
    {
        ArgumentNullException.ThrowIfNull(mailboxAddressesList);
        var snapshot = mailboxAddresses.ToAddressList(paramName);

        if (mailboxAddressesList is List<MailboxAddress> list)
            list.AddRange(snapshot);
        else
            foreach (var mailboxAddress in snapshot)
                mailboxAddressesList.Add(mailboxAddress);
    }

    private static bool IsAddressParseException(Exception exception)
    {
        var exceptionTypeName = exception.GetType()
            .Name;
        return exception is ArgumentException or FormatException || string.Equals(exceptionTypeName, "ParseException", StringComparison.Ordinal);
    }

    private static void EnsureCompleteEmailAddress(MailboxAddress mailboxAddress, string paramName, int? index = null)
    {
        var address = mailboxAddress.Address.AsSpan()
            .Trim();
        var atIndex = address.IndexOf('@');
        if (atIndex > 0 && atIndex < address.Length - 1)
            return;

        var message = index.HasValue ? $"The email address at index {index.Value} is invalid. {AddressFormatHint}" : $"The email address is invalid. {AddressFormatHint}";
        throw new ArgumentException(message, paramName);
    }
}
