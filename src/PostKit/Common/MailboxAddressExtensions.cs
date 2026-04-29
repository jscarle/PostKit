using MimeKit;

namespace PostKit.Common;

internal static class MailboxAddressExtensions
{
    public static IList<MailboxAddress> ToAddressList(this string address)
    {
        var mailboxAddress = MailboxAddress.Parse(address);
        return [mailboxAddress];
    }

    public static IList<MailboxAddress> ToAddressList(this (string Address, string? Name) addressAndName)
    {
        var mailboxAddress = new MailboxAddress(addressAndName.Name, addressAndName.Address);
        return [mailboxAddress];
    }

    public static IList<MailboxAddress> ToAddressList(this MailboxAddress mailboxAddress)
    {
        ArgumentNullException.ThrowIfNull(mailboxAddress);
        return [mailboxAddress];
    }

    public static IList<MailboxAddress> ToAddressList(this IEnumerable<string> addresses)
    {
        ArgumentNullException.ThrowIfNull(addresses);
        var mailboxAddresses = addresses.Select(MailboxAddress.Parse);
        return [..mailboxAddresses];
    }

    public static IList<MailboxAddress> ToAddressList(this IEnumerable<MailboxAddress> mailboxAddresses)
    {
        ArgumentNullException.ThrowIfNull(mailboxAddresses);

        var list = new List<MailboxAddress>();
        foreach (var mailboxAddress in mailboxAddresses)
        {
            ArgumentNullException.ThrowIfNull(mailboxAddress);
            list.Add(mailboxAddress);
        }

        return list;
    }

    public static void AddRange(this IList<MailboxAddress> mailboxAddressesList, IEnumerable<MailboxAddress> mailboxAddresses)
    {
        ArgumentNullException.ThrowIfNull(mailboxAddressesList);
        ArgumentNullException.ThrowIfNull(mailboxAddresses);

        if (mailboxAddressesList is List<MailboxAddress> list)
            foreach (var mailboxAddress in mailboxAddresses)
            {
                ArgumentNullException.ThrowIfNull(mailboxAddress);
                list.Add(mailboxAddress);
            }
        else
            foreach (var mailboxAddress in mailboxAddresses)
            {
                ArgumentNullException.ThrowIfNull(mailboxAddress);
                mailboxAddressesList.Add(mailboxAddress);
            }
    }
}
