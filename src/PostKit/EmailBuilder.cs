// ReSharper disable RedundantExtendsListEntry
// Intentional reference as the code is separated into multiple files.

namespace PostKit;

/// <summary>Provides a fluent interface for constructing <see cref="Email"/> messages.</summary>
public sealed partial class EmailBuilder : IEmailBuilder
{
    private const int MessageSizeLimitInBytes = 10 * 1024 * 1024; // 10 MB

    internal EmailBuilder()
    {
    }

    /// <inheritdoc/>
    public Email Build()
    {
        if (_from is null)
            throw new InvalidOperationException("From address is required.");

        if (_to is null && _cc is null && _bcc is null)
            throw new InvalidOperationException("At least one recipient is required.");

        var totalRecipients = (_to?.Count ?? 0) + (_cc?.Count ?? 0) + (_bcc?.Count ?? 0);
        if (totalRecipients > 50)
            throw new InvalidOperationException("There are too many recipients. Postmark implements a limit of 50 recipients per message. The recipient count includes all To, Cc, and Bcc recipients combined.");
        
        if (_subject is null && !_templateId.HasValue && _templateAlias is null)
            throw new InvalidOperationException("Either a subject, or a template ID or alias, is required.");

        if (_textBody is null && _htmlBody is null && !_templateId.HasValue && _templateAlias is null)
            throw new InvalidOperationException("Either a text or HTML body, or a template ID or alias, is required.");

        if ((_htmlBody is not null || _textBody is not null) && (_templateId.HasValue || _templateAlias is not null))
            throw new InvalidOperationException("Neither a text or HTML body, nor a subject may be specified when using a template.");

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
