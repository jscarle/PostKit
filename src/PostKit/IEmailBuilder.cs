using System.Diagnostics.CodeAnalysis;
using MimeKit;

namespace PostKit;

[SuppressMessage("Naming", "CA1716: Identifiers should not match keywords", Justification = "Emails have to go to someone!")]
public interface IEmailBuilder
{
    Email Build();
    IEmailBuilder From(string address);
    IEmailBuilder From(string name, string address);
    IEmailBuilder From(MailboxAddress mailboxAddress);
    IEmailReplyToBuilder ReplyTo(string address);
    IEmailReplyToBuilder ReplyTo(string name, string address);
    IEmailReplyToBuilder ReplyTo(MailboxAddress mailboxAddress);
    IEmailReplyToBuilder ReplyTo(IEnumerable<string> addresses);
    IEmailReplyToBuilder ReplyTo(IList<MailboxAddress> mailboxAddresses);
    IEmailToBuilder To(string address);
    IEmailToBuilder To(string name, string address);
    IEmailToBuilder To(MailboxAddress mailboxAddress);
    IEmailToBuilder To(IEnumerable<string> addresses);
    IEmailToBuilder To(IList<MailboxAddress> mailboxAddresses);
    IEmailCcBuilder Cc(string address);
    IEmailCcBuilder Cc(string name, string address);
    IEmailCcBuilder Cc(MailboxAddress mailboxAddress);
    IEmailCcBuilder Cc(IEnumerable<string> addresses);
    IEmailCcBuilder Cc(IList<MailboxAddress> mailboxAddresses);
    IEmailBccBuilder Bcc(string address);
    IEmailBccBuilder Bcc(string name, string address);
    IEmailBccBuilder Bcc(MailboxAddress mailboxAddress);
    IEmailBccBuilder Bcc(IEnumerable<string> addresses);
    IEmailBccBuilder Bcc(IList<MailboxAddress> mailboxAddresses);
    IEmailBuilder WithSubject(string subject);
    IEmailBuilder WithHtmlBody(string html);
    IEmailBuilder WithTextBody(string text);
    IEmailBuilder WithTag(string tag);
    IEmailBuilder WithHeader(string name, string value);
    IEmailBuilder WithHeader(KeyValuePair<string, string> header);
    IEmailBuilder WithHeader(IEnumerable<KeyValuePair<string, string>> headers);
    IEmailBuilder WithHeader(IDictionary<string, string> headers);
    IEmailBuilder WithMetadata(string name, string value);
    IEmailBuilder WithMetadata(KeyValuePair<string, string> entry);
    IEmailBuilder WithMetadata(IEnumerable<KeyValuePair<string, string>> metadata);
    IEmailBuilder WithMetadata(IDictionary<string, string> metadata);
    IEmailBuilder WithAttachment(Attachment attachment);
    IEmailBuilder WithAttachments(IEnumerable<Attachment> attachments);
    IEmailBuilder WithOpenTracking(bool openTracking = true);
    IEmailBuilder WithLinkTracking(LinkTracking linkTracking = LinkTracking.HtmlAndText);
    IEmailBuilder UsingMessageStream(MessageStream messageStream);
    IEmailBuilder UsingMessageStream(string messageStreamId);
}
