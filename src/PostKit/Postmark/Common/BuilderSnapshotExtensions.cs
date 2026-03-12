using System.Collections.ObjectModel;
using MimeKit;

namespace PostKit.Postmark.Common;

internal static class BuilderSnapshotExtensions
{
    public static MailboxAddress Snapshot(this MailboxAddress mailboxAddress)
    {
        return new MailboxAddress(mailboxAddress.Name, mailboxAddress.Address);
    }

    public static IReadOnlyCollection<MailboxAddress> SnapshotReadOnly(this IEnumerable<MailboxAddress> mailboxAddresses)
    {
        return mailboxAddresses.Select(static mailboxAddress => mailboxAddress.Snapshot())
            .ToList()
            .AsReadOnly();
    }

    public static IReadOnlyDictionary<string, string> SnapshotReadOnly(this IEnumerable<KeyValuePair<string, string>> values)
    {
        return new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(values, StringComparer.OrdinalIgnoreCase));
    }
}
