using MimeKit;

namespace PostKit;

public sealed partial class EmailBuilder
{
    internal EmailBuilder()
    {
    }

    public Email Build()
    {
        if (_from is null)
            throw new InvalidOperationException("From address is required.");
            
        if (_to.Count == 0)
            throw new InvalidOperationException("A To address is required.");

        var totalRecipients = _to.Count + (_cc?.Count ?? 0) + (_bcc?.Count ?? 0);
        if (totalRecipients > 50)
            throw new InvalidOperationException("There are too many recipients. Postmark implements a limit of 50 recipients per message. The recipient count includes all To, Cc, and Bcc recipients combined.");
        
        if (_htmlBody is null && _textBody is null)
            throw new InvalidOperationException("Either an HTML or text body is required.");
            
        var email = new Email
        {
            From = _from,
            ReplyTo = _replyTo,
            To = _to,
            Cc = _cc,
            Bcc = _bcc,
            Subject = _subject,
            HtmlBody = _htmlBody,
            TextBody = _textBody,
        };
        
        return email;
    }
}
