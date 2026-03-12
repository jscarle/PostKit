// ReSharper disable RedundantExtendsListEntry
// Intentional reference as the code is separated into multiple files.

using PostKit.Postmark.Common;

namespace PostKit.Emails;

/// <summary>Provides a fluent interface for constructing <see cref="Email"/> messages.</summary>
public sealed partial class EmailBuilder : IEmailBuilder
{
    internal EmailBuilder()
    {
    }

    /// <inheritdoc/>
    public Email Build()
    {
        if (_from is null)
            throw new InvalidOperationException("From address is required.");

        var totalRecipients = (_to?.Count ?? 0) + (_cc?.Count ?? 0) + (_bcc?.Count ?? 0);
        if (totalRecipients == 0)
            throw new InvalidOperationException("At least one recipient is required.");

        if (totalRecipients > 50)
            throw new InvalidOperationException("There are too many recipients. Postmark implements a limit of 50 recipients per message. The recipient count includes all To, Cc, and Bcc recipients combined.");

        if (_subject is null && !_templateId.HasValue && _templateAlias is null)
            throw new InvalidOperationException("Either a subject, or a template ID or alias, is required.");

        if (_textBody is null && _htmlBody is null && !_templateId.HasValue && _templateAlias is null)
            throw new InvalidOperationException("Either a text or HTML body, or a template ID or alias, is required.");

        if ((_htmlBody is not null || _textBody is not null) && (_templateId.HasValue || _templateAlias is not null))
            throw new InvalidOperationException("Neither a text or HTML body, nor a subject may be specified when using a template.");

        if ((_templateId.HasValue || _templateAlias is not null) && _templateModel is null)
            throw new InvalidOperationException("A template model is required when using a template.");

        var textBodySize = PostmarkSizeEstimator.EstimateBodySizeLowerBound(_textBody);
        if (textBodySize > PostmarkSizeEstimator.BodySizeLimitInBytes)
            throw new InvalidOperationException("Text body exceeds Postmark's 5 MB limit.");

        var htmlBodySize = PostmarkSizeEstimator.EstimateBodySizeLowerBound(_htmlBody);
        if (htmlBodySize > PostmarkSizeEstimator.BodySizeLimitInBytes)
            throw new InvalidOperationException("HTML body exceeds Postmark's 5 MB limit.");

        var projectedTotal = textBodySize + htmlBodySize + _attachmentBytes;
        if (projectedTotal > PostmarkSizeEstimator.MessageSizeLimitInBytes)
            throw new InvalidOperationException("Message size exceeds Postmark's 10 MB limit.");

        var email = new Email
        {
            From = _from.Snapshot(),
            ReplyTo = _replyTo?.SnapshotReadOnly(),
            To = _to?.SnapshotReadOnly(),
            Cc = _cc?.SnapshotReadOnly(),
            Bcc = _bcc?.SnapshotReadOnly(),
            Subject = _subject,
            HtmlBody = _htmlBody,
            TextBody = _textBody,
            Tag = _tag,
            Headers = _headers?.SnapshotReadOnly(),
            Metadata = _metadata?.SnapshotReadOnly(),
            OpenTracking = _openTracking,
            LinkTracking = _linkTracking,
            MessageStream = _messageStream,
            Attachments = _attachments?.ToList()
                .AsReadOnly(),
            TemplateId = _templateId,
            TemplateAlias = _templateAlias,
            TemplateModel = _templateModel,
            InlineCss = _inlineCss,
        };

        return email;
    }
}
