using MimeKit;

namespace PostKit;

public interface IEmailBccBuilder : IEmailBuilder
{
    IEmailBccBuilder AlsoBcc(string address);
    IEmailBccBuilder AlsoBcc(string name, string address);
    IEmailBccBuilder AlsoBcc(MailboxAddress mailboxAddress);
    IEmailBccBuilder AlsoBcc(IEnumerable<string> addresses);
    IEmailBccBuilder AlsoBcc(IList<MailboxAddress> mailboxAddresses);
}
