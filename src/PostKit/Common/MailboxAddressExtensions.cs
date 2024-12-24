using MimeKit;

namespace PostKit.Common;

internal static class MailboxAddressExtensions
{
    public static IList<MailboxAddress> ToAddressList(this string address)
    {
        var mailboxAddress = MailboxAddress.Parse(address);
        return [mailboxAddress];
    }

    public static IList<MailboxAddress> ToAddressList(this (string Name, string Address) nameAndAddress)
    {
        var mailboxAddress = new MailboxAddress(nameAndAddress.Name, nameAndAddress.Address);
        return [mailboxAddress];
    }

    public static IList<MailboxAddress> ToAddressList(this MailboxAddress mailboxAddress)
    {
        return [mailboxAddress];
    }

    public static IList<MailboxAddress> ToAddressList(this IEnumerable<string> addresses)
    {
        var mailboxAddresses = addresses.Select(MailboxAddress.Parse);
        return [..mailboxAddresses];
    }
    
    public static void AddRange(this IList<MailboxAddress> mailboxAddressesList, IList<MailboxAddress> mailboxAddresses)
    {
        if (mailboxAddressesList is List<MailboxAddress> list)
        {
            list.AddRange(mailboxAddresses);
        }
        else
        {
            foreach(var mailboxAddress in mailboxAddresses)
                mailboxAddressesList.Add(mailboxAddress);
        }
    }
}
