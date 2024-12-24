using MimeKit;

namespace PostKit;

public sealed class Email
{
    public MailboxAddress? From { get; internal init; }

    public IReadOnlyCollection<MailboxAddress>? ReplyTo { get; internal init; }

    public IReadOnlyCollection<MailboxAddress>? To { get; internal init; }

    public IReadOnlyCollection<MailboxAddress>? Cc { get; internal init; }

    public IReadOnlyCollection<MailboxAddress>? Bcc { get; internal init; }

    public string? Subject { get; internal init; }

    public string? HtmlBody { get; internal init; }

    public string? TextBody { get; internal init; }

    public string? Tag { get; internal init; }

    public IReadOnlyDictionary<string, string>? Headers { get; internal init; }

    public IReadOnlyDictionary<string, string>? Metadata { get; internal init; }

    public bool? OpenTracking { get; internal init; }

    public LinkTracking? LinkTracking { get; internal init; }

    public string? MessageStream { get; internal init; }

    internal Email()
    {
    }

    public static EmailBuilder CreateBuilder()
    {
        return new EmailBuilder();
    }
}
