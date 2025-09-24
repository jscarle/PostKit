// ReSharper disable RedundantExtendsListEntry
// Intentional reference as the code is separated into multiple files.

namespace PostKit;

public sealed partial class EmailBuilder : IEmailBuilder
{
    internal EmailBuilder()
    {
    }

    public Email Build()
    {
        if (_from is null)
            throw new InvalidOperationException("From address is required.");

        if (_to is null && _cc is null && _bcc is null)
            throw new InvalidOperationException("At least one recipient is required.");

        var totalRecipients = (_to?.Count ?? 0) + (_cc?.Count ?? 0) + (_bcc?.Count ?? 0);
        if (totalRecipients > 50)
            throw new InvalidOperationException("There are too many recipients. Postmark implements a limit of 50 recipients per message. The recipient count includes all To, Cc, and Bcc recipients combined.");

        if (_htmlBody is null && _textBody is null)
            throw new InvalidOperationException("Either an HTML or text body is required.");

        var email = new Email
        {
            From = _from,
            ReplyTo = _replyTo?.AsReadOnly(),
            To = _to?.AsReadOnly(),
            Cc = _cc?.AsReadOnly(),
            Bcc = _bcc?.AsReadOnly(),
            Subject = _subject,
            HtmlBody = _htmlBody,
            TextBody = _textBody,
            Tag = _tag,
            Headers = _headers?.AsReadOnly(),
            Metadata = _metadata?.AsReadOnly(),
            LinkTracking = _linkTracking,
            MessageStream = _messageStream,
        };

        return email;
    }
}
