using MimeKit;

namespace PostKit;

public sealed class Email
{
    public required MailboxAddress From { get; init; }

    public IReadOnlyCollection<MailboxAddress>? ReplyTo { get; init; }

    public required IReadOnlyCollection<MailboxAddress> To { get; init; }

    public IReadOnlyCollection<MailboxAddress>? Cc { get; init; }

    public IReadOnlyCollection<MailboxAddress>? Bcc { get; init; }

    public string? Subject { get; init; }

    public string? HtmlBody { get; init; }
    
    public string? TextBody { get; init; }

    internal Email()
    {
    }

    public static EmailBuilder CreateBuilder()
    {
        return new EmailBuilder();
    }
}
