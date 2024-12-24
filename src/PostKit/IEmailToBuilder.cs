using MimeKit;

namespace PostKit;

public interface IEmailToBuilder : IEmailBuilder
{
    IEmailToBuilder AlsoTo(string address);
    IEmailToBuilder AlsoTo(string name, string address);
    IEmailToBuilder AlsoTo(MailboxAddress mailboxAddress);
    IEmailToBuilder AlsoTo(IEnumerable<string> addresses);
    IEmailToBuilder AlsoTo(IList<MailboxAddress> mailboxAddresses);
}
