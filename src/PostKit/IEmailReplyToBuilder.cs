using MimeKit;

namespace PostKit;

public interface IEmailReplyToBuilder : IEmailBuilder
{
    IEmailReplyToBuilder AlsoReplyTo(string address);
    IEmailReplyToBuilder AlsoReplyTo(string name, string address);
    IEmailReplyToBuilder AlsoReplyTo(MailboxAddress mailboxAddress);
    IEmailReplyToBuilder AlsoReplyTo(IEnumerable<string> addresses);
    IEmailReplyToBuilder AlsoReplyTo(IList<MailboxAddress> mailboxAddresses);
}
