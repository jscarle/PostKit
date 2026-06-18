using System.Diagnostics;
using System.Text.RegularExpressions;
using MimeKit;
using PostKit.Common;

namespace PostKit.BulkEmails;

internal sealed partial class DraftBulkEmail
{
    private const string TemplateAliasRegexPattern = "^[A-Za-z][A-Za-z0-9._-]*$";
#if NET9_0_OR_GREATER
    [GeneratedRegex(TemplateAliasRegexPattern)]
    private static partial Regex TemplateAliasRegex { get; }
#else
    [GeneratedRegex(TemplateAliasRegexPattern)]
    private static partial Regex TemplateAliasRegex();
#endif

    private const int TemplateAliasMaxLength = 64;
    private const string TemplateContentConflictGuidance = "Use either template fields (TemplateId or TemplateAlias) or content fields (Subject with TextBody or HtmlBody).";

    private MailboxAddress? _from;
    private IList<MailboxAddress>? _replyTo;
    private string? _subject;
    private string? _htmlBody;
    private string? _textBody;
    private string? _tag;
    private Dictionary<string, string>? _headers;
    private Dictionary<string, string>? _metadata;
    private bool? _openTracking;
    private LinkTracking? _linkTracking;
    private string? _messageStream;
    private List<Attachment>? _attachments;
    private int? _templateId;
    private string? _templateAlias;
    private bool? _inlineCss;
    private readonly List<BulkEmailMessage> _messages = [];

    public DraftBulkEmail From(string address)
    {
        _from.EnsureNotSet(nameof(BulkEmail.From));

        var mailboxAddress = address.ToMailboxAddress();

        ValidateFrom(mailboxAddress, nameof(address));

        _from = mailboxAddress.Snapshot();

        return this;
    }

    public DraftBulkEmail From(string address, string? name)
    {
        _from.EnsureNotSet(nameof(BulkEmail.From));
        ValidationExtensions.EnsureAddressFirst(address, name);

        var mailboxAddress = (address, name).ToMailboxAddress();

        ValidateFrom(mailboxAddress, nameof(address));

        _from = mailboxAddress.Snapshot();

        return this;
    }

    public DraftBulkEmail From(MailboxAddress mailboxAddress)
    {
        if (mailboxAddress is null)
            throw new ArgumentNullException(nameof(mailboxAddress), "The from address cannot be null.");

        _from.EnsureNotSet(nameof(BulkEmail.From));

        ValidateFrom(mailboxAddress, nameof(mailboxAddress));

        _from = mailboxAddress.Snapshot();

        return this;
    }

    public DraftBulkEmail ReplyTo(string address)
    {
        return AddReplyTo(address.ToAddressList());
    }

    public DraftBulkEmail ReplyTo(string address, string? name)
    {
        ValidationExtensions.EnsureAddressFirst(address, name);
        return AddReplyTo((address, name).ToAddressList());
    }

    public DraftBulkEmail ReplyTo(MailboxAddress mailboxAddress)
    {
        return AddReplyTo(mailboxAddress.ToAddressList());
    }

    public DraftBulkEmail ReplyTo(IEnumerable<string> addresses)
    {
        return AddReplyTo(addresses.ToAddressList());
    }

    public DraftBulkEmail ReplyTo(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        return AddReplyTo(mailboxAddresses.ToAddressList());
    }

    public DraftBulkEmail ReplyTo(IList<MailboxAddress> mailboxAddresses)
    {
        return AddReplyTo(mailboxAddresses.ToAddressList());
    }

    public DraftBulkEmail Subject(string subject)
    {
        if (subject is null)
            throw new ArgumentNullException(nameof(subject), "The subject cannot be null.");

        _subject.EnsureNotSet(nameof(BulkEmail.Subject));
        _templateId.EnsureNotSet(nameof(BulkEmail.TemplateId), nameof(BulkEmail.Subject), TemplateContentConflictGuidance);
        _templateAlias.EnsureNotSet(nameof(BulkEmail.TemplateAlias), nameof(BulkEmail.Subject), TemplateContentConflictGuidance);

        var length = subject.AsSpan()
            .GetPostmarkCharacterCount();
        if (length > 2000)
            throw new ArgumentException($"The subject cannot be longer than 2000 characters. Actual length: {length}.", nameof(subject));

        _subject = subject;

        return this;
    }

    public DraftBulkEmail HtmlBody(string htmlBody)
    {
        if (htmlBody is null)
            throw new ArgumentNullException(nameof(htmlBody), "The HTML body cannot be null.");

        _templateId.EnsureNotSet(nameof(BulkEmail.TemplateId), nameof(BulkEmail.HtmlBody), TemplateContentConflictGuidance);
        _templateAlias.EnsureNotSet(nameof(BulkEmail.TemplateAlias), nameof(BulkEmail.HtmlBody), TemplateContentConflictGuidance);
        _inlineCss.EnsureNotSet(nameof(BulkEmail.InlineCss), nameof(BulkEmail.HtmlBody), TemplateContentConflictGuidance);
        _htmlBody.EnsureNotSet(nameof(BulkEmail.HtmlBody));

        _htmlBody = htmlBody;

        return this;
    }

    public DraftBulkEmail TextBody(string textBody)
    {
        if (textBody is null)
            throw new ArgumentNullException(nameof(textBody), "The text body cannot be null.");

        _templateId.EnsureNotSet(nameof(BulkEmail.TemplateId), nameof(BulkEmail.TextBody), TemplateContentConflictGuidance);
        _templateAlias.EnsureNotSet(nameof(BulkEmail.TemplateAlias), nameof(BulkEmail.TextBody), TemplateContentConflictGuidance);
        _inlineCss.EnsureNotSet(nameof(BulkEmail.InlineCss), nameof(BulkEmail.TextBody), TemplateContentConflictGuidance);
        _textBody.EnsureNotSet(nameof(BulkEmail.TextBody));

        _textBody = textBody;

        return this;
    }

    public DraftBulkEmail WithTag(string tag)
    {
        _tag.EnsureNotSet(nameof(BulkEmail.Tag));
        if (tag is null)
            throw new ArgumentNullException(nameof(tag), "The tag cannot be null.");

        if (tag.Length > 1000)
            throw new ArgumentException($"The tag cannot be longer than 1000 characters. Actual length: {tag.Length}.", nameof(tag));

        _tag = tag;

        return this;
    }

    public DraftBulkEmail AddHeader(string name, string value)
    {
        ValidationExtensions.ValidateHeaderName(name, nameof(name));
        ValidationExtensions.ValidateHeaderValue(value, nameof(value));

        AddHeaderEntry(name, value, nameof(name));

        return this;
    }

    public DraftBulkEmail AddHeader(KeyValuePair<string, string> header)
    {
        ValidationExtensions.ValidateHeader(header.Key, header.Value, nameof(header));

        AddHeaderEntry(header.Key, header.Value, nameof(header));

        return this;
    }

    public DraftBulkEmail AddHeader(IEnumerable<KeyValuePair<string, string>> headers)
    {
        var headerList = ValidationExtensions.SnapshotValidatedHeaders(headers, nameof(headers), _headers);

        foreach (var header in headerList)
            AddHeaderEntry(header.Key, header.Value, nameof(headers));

        return this;
    }

    public DraftBulkEmail AddHeader(IDictionary<string, string> headers)
    {
        var headerList = ValidationExtensions.SnapshotValidatedHeaders(headers, nameof(headers), _headers);

        foreach (var header in headerList)
            AddHeaderEntry(header.Key, header.Value, nameof(headers));

        return this;
    }

    public DraftBulkEmail AddMetadata(string name, string value)
    {
        ValidationExtensions.ValidateMetadataName(name, nameof(name));
        ValidationExtensions.ValidateMetadataValue(value, nameof(value));

        AddMetadataEntry(name, value, nameof(name));

        return this;
    }

    public DraftBulkEmail AddMetadata(KeyValuePair<string, string> entry)
    {
        ValidationExtensions.ValidateMetadata(entry.Key, entry.Value, nameof(entry));

        AddMetadataEntry(entry.Key, entry.Value, nameof(entry));

        return this;
    }

    public DraftBulkEmail AddMetadata(IEnumerable<KeyValuePair<string, string>> metadata)
    {
        var metadataList = ValidationExtensions.SnapshotValidatedMetadata(metadata, nameof(metadata), _metadata);

        foreach (var entry in metadataList)
            AddMetadataEntry(entry.Key, entry.Value, nameof(metadata));

        return this;
    }

    public DraftBulkEmail AddMetadata(IDictionary<string, string> metadata)
    {
        var metadataList = ValidationExtensions.SnapshotValidatedMetadata(metadata, nameof(metadata), _metadata);

        foreach (var entry in metadataList)
            AddMetadataEntry(entry.Key, entry.Value, nameof(metadata));

        return this;
    }

    public DraftBulkEmail EnableOpenTracking()
    {
        _openTracking.EnsureNotSet(nameof(BulkEmail.OpenTracking));

        _openTracking = true;

        return this;
    }

    public DraftBulkEmail UseLinkTracking(LinkTracking linkTracking = LinkTracking.HtmlAndText)
    {
        _linkTracking.EnsureNotSet(nameof(BulkEmail.LinkTracking));

        _linkTracking = linkTracking;

        return this;
    }

    public DraftBulkEmail UseMessageStream(MessageStream messageStream)
    {
        _messageStream.EnsureNotSet(nameof(BulkEmail.MessageStream));

        _messageStream = messageStream switch
        {
            MessageStream.Broadcast => "broadcast",
            MessageStream.Transactional => throw new ArgumentException("Bulk sends require a broadcast message stream. Received MessageStream.Transactional; use MessageStream.Broadcast or a broadcast stream ID.", nameof(messageStream)),
            _ => throw new UnreachableException($"Enum value of '{nameof(MessageStream)}.{messageStream}' has not been handled.")
        };

        return this;
    }

    public DraftBulkEmail UseMessageStream(string messageStreamId)
    {
        _messageStream.EnsureNotSet(nameof(BulkEmail.MessageStream));

        if (string.Equals(messageStreamId, "outbound", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"Bulk sends require a broadcast message stream. Received '{messageStreamId}'; use MessageStream.Broadcast or a broadcast stream ID.", nameof(messageStreamId));

        ValidationExtensions.ValidateMessageStreamId(messageStreamId, nameof(messageStreamId));

        _messageStream = messageStreamId;

        return this;
    }

    public DraftBulkEmail AddAttachment(Attachment attachment)
    {
        if (attachment is null)
            throw new ArgumentNullException(nameof(attachment), "The attachment cannot be null.");

        var estimatedSize = PostmarkSizeEstimator.EstimateBase64SizeLowerBound(attachment.Content);
        EnsureAttachmentsWithinLimit(estimatedSize);

        (_attachments ??= []).Add(attachment);

        return this;
    }

    public DraftBulkEmail AddAttachment(IEnumerable<Attachment> attachments)
    {
        if (attachments is null)
            throw new ArgumentNullException(nameof(attachments), "The attachments collection cannot be null.");

        var buffer = attachments as ICollection<Attachment> ?? attachments.ToList();
        if (buffer.Count == 0)
            return this;

        long additionalBytes = 0;
        var index = 0;
        foreach (var attachment in buffer)
        {
            if (attachment is null)
                throw new ArgumentException($"The attachment at index {index} cannot be null.", nameof(attachments));

            additionalBytes += PostmarkSizeEstimator.EstimateBase64SizeLowerBound(attachment.Content);
            index++;
        }

        EnsureAttachmentsWithinLimit(additionalBytes);

        (_attachments ??= []).AddRange(buffer);

        return this;
    }

    public DraftBulkEmail AddMessage(BulkEmailMessage message)
    {
        if (message is null)
            throw new ArgumentNullException(nameof(message), "The bulk email message cannot be null.");

        _messages.Add(message);

        return this;
    }

    public DraftBulkEmail AddMessage(IEnumerable<BulkEmailMessage> messages)
    {
        if (messages is null)
            throw new ArgumentNullException(nameof(messages), "The bulk email message collection cannot be null.");

        var buffer = messages as ICollection<BulkEmailMessage> ?? messages.ToList();
        if (buffer.Count == 0)
            return this;

        var index = 0;
        foreach (var message in buffer)
        {
            if (message is null)
                throw new ArgumentException($"The bulk email message at index {index} cannot be null.", nameof(messages));

            index++;
        }

        _messages.AddRange(buffer);

        return this;
    }

    public DraftBulkEmail SetTemplate(int templateId, bool? inlineCss = null)
    {
        _subject.EnsureNotSet(nameof(BulkEmail.Subject), nameof(BulkEmail.TemplateId), TemplateContentConflictGuidance);
        _htmlBody.EnsureNotSet(nameof(BulkEmail.HtmlBody), nameof(BulkEmail.TemplateId), TemplateContentConflictGuidance);
        _textBody.EnsureNotSet(nameof(BulkEmail.TextBody), nameof(BulkEmail.TemplateId), TemplateContentConflictGuidance);
        _templateId.EnsureNotSet(nameof(BulkEmail.TemplateId));
        _templateAlias.EnsureNotSet(nameof(BulkEmail.TemplateAlias), nameof(BulkEmail.TemplateId), "Only one template identifier can be set.");
        _inlineCss.EnsureNotSet(nameof(BulkEmail.InlineCss), nameof(BulkEmail.TemplateId));

        if (templateId <= 0)
            throw new ArgumentException($"The template ID must be greater than zero. Received {templateId}.", nameof(templateId));

        _templateId = templateId;
        _inlineCss = inlineCss;

        return this;
    }

    public DraftBulkEmail SetTemplate(string templateAlias, bool? inlineCss = null)
    {
        _subject.EnsureNotSet(nameof(BulkEmail.Subject), nameof(BulkEmail.TemplateAlias), TemplateContentConflictGuidance);
        _htmlBody.EnsureNotSet(nameof(BulkEmail.HtmlBody), nameof(BulkEmail.TemplateAlias), TemplateContentConflictGuidance);
        _textBody.EnsureNotSet(nameof(BulkEmail.TextBody), nameof(BulkEmail.TemplateAlias), TemplateContentConflictGuidance);
        _templateId.EnsureNotSet(nameof(BulkEmail.TemplateId), nameof(BulkEmail.TemplateAlias), "Only one template identifier can be set.");
        _templateAlias.EnsureNotSet(nameof(BulkEmail.TemplateAlias));
        _inlineCss.EnsureNotSet(nameof(BulkEmail.InlineCss), nameof(BulkEmail.TemplateAlias));

        if (templateAlias is null)
            throw new ArgumentNullException(nameof(templateAlias), "The template alias cannot be null.");

        if (string.IsNullOrWhiteSpace(templateAlias))
            throw new ArgumentException("The template alias is required.", nameof(templateAlias));

        if (templateAlias.Length > TemplateAliasMaxLength)
            throw new ArgumentException($"The template alias must not exceed {TemplateAliasMaxLength} characters. Actual length: {templateAlias.Length}.", nameof(templateAlias));
#if NET9_0_OR_GREATER
        if (!TemplateAliasRegex.IsMatch(templateAlias))
#else
        if (!TemplateAliasRegex().IsMatch(templateAlias))
#endif
            throw new ArgumentException($"The template alias must start with a letter and may only contain letters, numbers, '-', '_', or '.' characters. {GetTemplateAliasValidationDetail(templateAlias)}", nameof(templateAlias));

        _templateAlias = templateAlias;
        _inlineCss = inlineCss;

        return this;
    }

    public BulkEmail Build()
    {
        if (_from is null)
            throw new InvalidOperationException("From address is required before building the bulk email. Call From(...).");

        if (_messages.Count == 0)
            throw new InvalidOperationException("At least one message is required before building the bulk email. Call AddMessage(...).");

        if (_subject is null && !_templateId.HasValue && _templateAlias is null)
            throw new InvalidOperationException("Subject is required before building the bulk email. Call Subject(...).");

        if (_textBody is null && _htmlBody is null && !_templateId.HasValue && _templateAlias is null)
            throw new InvalidOperationException("Message content is required before building the bulk email. Call TextBody(...) or HtmlBody(...).");

        if ((_htmlBody is not null || _textBody is not null || _subject is not null) && (_templateId.HasValue || _templateAlias is not null))
            throw new InvalidOperationException("Bulk template emails cannot also set Subject, TextBody, or HtmlBody. Use either template fields (TemplateId or TemplateAlias) or content fields (Subject with TextBody or HtmlBody).");

        EnsureMergedHeadersDoNotDuplicate();
        EnsureMergedMetadataWithinLimit();

        var textBodySize = PostmarkSizeEstimator.EstimateBodySizeLowerBound(_textBody);
        if (textBodySize > PostmarkSizeEstimator.BodySizeLimitInBytes)
            throw new InvalidOperationException(PostmarkSizeEstimator.FormatActualSizeLimitMessage("Text body exceeds Postmark's 5 MB limit.", textBodySize, PostmarkSizeEstimator.BodySizeLimitInBytes));

        var htmlBodySize = PostmarkSizeEstimator.EstimateBodySizeLowerBound(_htmlBody);
        if (htmlBodySize > PostmarkSizeEstimator.BodySizeLimitInBytes)
            throw new InvalidOperationException(PostmarkSizeEstimator.FormatActualSizeLimitMessage("HTML body exceeds Postmark's 5 MB limit.", htmlBodySize, PostmarkSizeEstimator.BodySizeLimitInBytes));

        EnsureMessageContentWithinLimit();

        var bulkEmail = new BulkEmail
        {
            From = _from.Snapshot(),
            ReplyTo = _replyTo?.SnapshotReadOnly(),
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
            InlineCss = _inlineCss,
            Messages = _messages.ToList()
                .AsReadOnly()
        };

        var estimatedTotal = PostmarkSizeEstimator.EstimateBulkEmailPayloadSizeLowerBound(bulkEmail);
        if (estimatedTotal > PostmarkSizeEstimator.BulkPayloadSizeLimitInBytes)
            throw new InvalidOperationException(PostmarkSizeEstimator.FormatEstimatedSizeLimitMessage("Estimated bulk request size exceeds Postmark's 50 MB limit.", estimatedTotal, PostmarkSizeEstimator.BulkPayloadSizeLimitInBytes));

        return bulkEmail;
    }

    private static void ValidateFrom(MailboxAddress mailboxAddress, string paramName)
    {
        var fromString = mailboxAddress.ToString(true);
        var length = fromString.AsSpan()
            .GetPostmarkCharacterCount();
        if (length > 255)
            throw new ArgumentException($"The {nameof(BulkEmail.From)} address cannot exceed 255 characters. Actual length: {length}.", paramName);
    }

    private static string GetTemplateAliasValidationDetail(string templateAlias)
    {
        if (templateAlias[0] is not (>= 'A' and <= 'Z' or >= 'a' and <= 'z'))
            return $"First character must be a letter. Received {ValidationExtensions.FormatCharacter(templateAlias[0])} at index 0.";

        for (var index = 1; index < templateAlias.Length; index++)
        {
            var current = templateAlias[index];
            var isLetter = current is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
            var isDigit = current is >= '0' and <= '9';
            var isAllowedPunctuation = current is '-' or '_' or '.';
            if (!isLetter && !isDigit && !isAllowedPunctuation)
                return $"Invalid character {ValidationExtensions.FormatCharacter(current)} at index {index}.";
        }

        return "The value is invalid.";
    }

    private DraftBulkEmail AddReplyTo(IEnumerable<MailboxAddress> replyTo)
    {
        var snapshot = replyTo.ToAddressList();

        if (_replyTo is null)
            _replyTo = snapshot;
        else
            _replyTo.AddRange(snapshot);

        return this;
    }

    private void AddHeaderEntry(string name, string value, string paramName)
    {
        if (_headers is not null && ValidationExtensions.TryGetExistingKey(_headers, name, out var existingName))
            throw new ArgumentException(ValidationExtensions.FormatDuplicateExistingHeaderMessage(name, existingName), paramName);

        (_headers ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)).Add(name, value);
    }

    private void AddMetadataEntry(string name, string value, string paramName)
    {
        if (_metadata is not null && ValidationExtensions.TryGetExistingKey(_metadata, name, out var existingName))
            throw new ArgumentException(ValidationExtensions.FormatDuplicateExistingMetadataMessage(name, existingName), paramName);

        var existingCount = _metadata?.Count ?? 0;
        if (existingCount >= 10)
            throw new InvalidOperationException($"Cannot set more than 10 metadata fields for a message. Adding 1 metadata field to the existing {existingCount} would produce {existingCount + 1}.");

        (_metadata ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)).Add(name, value);
    }

    private void EnsureAttachmentsWithinLimit(long additionalBytes)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(additionalBytes);

        var projectedTotal = PostmarkSizeEstimator.EstimateMessageContentSizeLowerBound(_textBody, _htmlBody, 0, _attachments, _headers) + additionalBytes;
        if (projectedTotal > PostmarkSizeEstimator.MessageSizeLimitInBytes)
            throw new InvalidOperationException(PostmarkSizeEstimator.FormatEstimatedSizeLimitMessage("Estimated message content exceeds Postmark's 10 MB limit.", projectedTotal, PostmarkSizeEstimator.MessageSizeLimitInBytes));
    }

    private void EnsureMessageContentWithinLimit()
    {
        var sharedContentSize = PostmarkSizeEstimator.EstimateMessageContentSizeLowerBound(_textBody, _htmlBody, 0, _attachments, _headers);
        if (sharedContentSize > PostmarkSizeEstimator.MessageSizeLimitInBytes)
            throw new InvalidOperationException(PostmarkSizeEstimator.FormatEstimatedSizeLimitMessage("Estimated message content exceeds Postmark's 10 MB limit.", sharedContentSize, PostmarkSizeEstimator.MessageSizeLimitInBytes));

        for (var index = 0; index < _messages.Count; index++)
        {
            var message = _messages[index];
            var projectedTotal = sharedContentSize + message.TemplateModelSizeInBytes + PostmarkSizeEstimator.EstimateHeaderSizeLowerBound(message.Headers);
            if (projectedTotal > PostmarkSizeEstimator.MessageSizeLimitInBytes)
                throw new InvalidOperationException(PostmarkSizeEstimator.FormatEstimatedSizeLimitMessage($"Estimated message content for bulk message at index {index} exceeds Postmark's 10 MB limit.", projectedTotal,
                    PostmarkSizeEstimator.MessageSizeLimitInBytes));
        }
    }

    private void EnsureMergedMetadataWithinLimit()
    {
        if (_metadata is null || _metadata.Count == 0)
            return;

        for (var messageIndex = 0; messageIndex < _messages.Count; messageIndex++)
        {
            var message = _messages[messageIndex];
            if (message.Metadata is not null)
            {
                foreach (var entry in message.Metadata)
                    if (ValidationExtensions.TryGetExistingKey(_metadata, entry.Key, out var existingName))
                        throw new InvalidOperationException(
                            $"Cannot use duplicate metadata names for bulk message at index {messageIndex} after combining request-level and message-level metadata. Metadata name '{entry.Key}' duplicates request-level metadata name '{existingName}'. Metadata names are compared case-insensitively.");

                var mergedMetadataCount = _metadata.Count + message.Metadata.Count;
                if (mergedMetadataCount > 10)
                    throw new InvalidOperationException(
                        $"Cannot set more than 10 metadata fields for bulk message at index {messageIndex} after combining request-level and message-level metadata. Request-level count: {_metadata.Count}; message-level count: {message.Metadata.Count}; combined count: {mergedMetadataCount}.");
            }
        }
    }

    private void EnsureMergedHeadersDoNotDuplicate()
    {
        if (_headers is null || _headers.Count == 0)
            return;

        for (var messageIndex = 0; messageIndex < _messages.Count; messageIndex++)
        {
            var message = _messages[messageIndex];
            if (message.Headers is null)
                continue;

            foreach (var entry in message.Headers)
                if (ValidationExtensions.TryGetExistingKey(_headers, entry.Key, out var existingName))
                    throw new InvalidOperationException(
                        $"Cannot use duplicate header names for bulk message at index {messageIndex} after combining request-level and message-level headers. Header name '{entry.Key}' duplicates request-level header name '{existingName}'. Header names are compared case-insensitively.");
        }
    }
}