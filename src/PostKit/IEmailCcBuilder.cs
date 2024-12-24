using MimeKit;

namespace PostKit;

public interface IEmailCcBuilder : IEmailBuilder
{
    IEmailCcBuilder AlsoCc(string address);
    IEmailCcBuilder AlsoCc(string name, string address);
    IEmailCcBuilder AlsoCc(MailboxAddress mailboxAddress);
    IEmailCcBuilder AlsoCc(IEnumerable<string> addresses);
    IEmailCcBuilder AlsoCc(IList<MailboxAddress> mailboxAddresses);
}
