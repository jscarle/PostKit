// ReSharper disable RedundantExtendsListEntry
// Intentional reference as the code is separated into multiple files.

namespace PostKit;

public sealed partial class EmailBuilder : IEmailBuilder
{
    private const int MessageSizeLimitInBytes = 10 * 1024 * 1024; // 10 MB
    
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

        if (_htmlBody is null && _textBody is null && !_templateId.HasValue && _templateAlias is null)
            throw new InvalidOperationException("Either an HTML or text body, or a template ID or alias, is required.");

        var bodyLength = (long)(_textBody?.Length ?? 0) + (_htmlBody?.Length ?? 0);
        var projectedTotal = _attachmentBytes + bodyLength;
        if (projectedTotal > MessageSizeLimitInBytes)
            throw new InvalidOperationException("Message size exceeds Postmark's 10 MB limit.");

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
            OpenTracking = _openTracking,
            LinkTracking = _linkTracking,
            MessageStream = _messageStream,
            Attachments = _attachments?.AsReadOnly(),
            TemplateId = _templateId,
            TemplateAlias = _templateAlias,
            TemplateModel = _templateModel,
            InlineCss = _inlineCss,
        };

        return email;
    }
}
