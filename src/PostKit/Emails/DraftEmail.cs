using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using MimeKit;
using PostKit.Common;

namespace PostKit.Emails;

internal sealed partial class DraftEmail
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
    private const string TemplateContentConflictGuidance = "Use either template fields (TemplateId or TemplateAlias with TemplateModel) or content fields (Subject with TextBody or HtmlBody).";

    private MailboxAddress? _from;
    private IList<MailboxAddress>? _replyTo;
    private IList<MailboxAddress>? _to;
    private IList<MailboxAddress>? _cc;
    private IList<MailboxAddress>? _bcc;
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
    private object? _templateModel;
    private JsonNode? _templateModelSnapshot;
    private int _templateModelSizeInBytes;
    private bool? _inlineCss;

    public DraftEmail From(string address)
    {
        _from.EnsureNotSet(nameof(Email.From));

        var mailboxAddress = address.ToMailboxAddress();

        ValidateFrom(mailboxAddress, nameof(address));

        _from = mailboxAddress.Snapshot();

        return this;
    }

    public DraftEmail From(string address, string? name)
    {
        _from.EnsureNotSet(nameof(Email.From));
        ValidationExtensions.EnsureAddressFirst(address, name);

        var mailboxAddress = (address, name).ToMailboxAddress();

        ValidateFrom(mailboxAddress, nameof(address));

        _from = mailboxAddress.Snapshot();

        return this;
    }

    public DraftEmail From(MailboxAddress mailboxAddress)
    {
        if (mailboxAddress is null)
            throw new ArgumentNullException(nameof(mailboxAddress), "The from address cannot be null.");

        _from.EnsureNotSet(nameof(Email.From));

        ValidateFrom(mailboxAddress, nameof(mailboxAddress));

        _from = mailboxAddress.Snapshot();

        return this;
    }

    public DraftEmail ReplyTo(string address)
    {
        return AddRecipients(ref _replyTo, address.ToAddressList());
    }

    public DraftEmail ReplyTo(string address, string? name)
    {
        ValidationExtensions.EnsureAddressFirst(address, name);
        return AddRecipients(ref _replyTo, (address, name).ToAddressList());
    }

    public DraftEmail ReplyTo(MailboxAddress mailboxAddress)
    {
        return AddRecipients(ref _replyTo, mailboxAddress.ToAddressList());
    }

    public DraftEmail ReplyTo(IEnumerable<string> addresses)
    {
        return AddRecipients(ref _replyTo, addresses.ToAddressList());
    }

    public DraftEmail ReplyTo(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        return AddRecipients(ref _replyTo, mailboxAddresses.ToAddressList());
    }

    public DraftEmail ReplyTo(IList<MailboxAddress> mailboxAddresses)
    {
        return AddRecipients(ref _replyTo, mailboxAddresses.ToAddressList());
    }

    public DraftEmail To(string address)
    {
        return AddRecipients(ref _to, address.ToAddressList());
    }

    public DraftEmail To(string address, string? name)
    {
        ValidationExtensions.EnsureAddressFirst(address, name);
        return AddRecipients(ref _to, (address, name).ToAddressList());
    }

    public DraftEmail To(MailboxAddress mailboxAddress)
    {
        return AddRecipients(ref _to, mailboxAddress.ToAddressList());
    }

    public DraftEmail To(IEnumerable<string> addresses)
    {
        return AddRecipients(ref _to, addresses.ToAddressList());
    }

    public DraftEmail To(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        return AddRecipients(ref _to, mailboxAddresses.ToAddressList());
    }

    public DraftEmail To(IList<MailboxAddress> mailboxAddresses)
    {
        return AddRecipients(ref _to, mailboxAddresses.ToAddressList());
    }

    public DraftEmail Cc(string address)
    {
        return AddRecipients(ref _cc, address.ToAddressList());
    }

    public DraftEmail Cc(string address, string? name)
    {
        ValidationExtensions.EnsureAddressFirst(address, name);
        return AddRecipients(ref _cc, (address, name).ToAddressList());
    }

    public DraftEmail Cc(MailboxAddress mailboxAddress)
    {
        return AddRecipients(ref _cc, mailboxAddress.ToAddressList());
    }

    public DraftEmail Cc(IEnumerable<string> addresses)
    {
        return AddRecipients(ref _cc, addresses.ToAddressList());
    }

    public DraftEmail Cc(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        return AddRecipients(ref _cc, mailboxAddresses.ToAddressList());
    }

    public DraftEmail Cc(IList<MailboxAddress> mailboxAddresses)
    {
        return AddRecipients(ref _cc, mailboxAddresses.ToAddressList());
    }

    public DraftEmail Bcc(string address)
    {
        return AddRecipients(ref _bcc, address.ToAddressList());
    }

    public DraftEmail Bcc(string address, string? name)
    {
        ValidationExtensions.EnsureAddressFirst(address, name);
        return AddRecipients(ref _bcc, (address, name).ToAddressList());
    }

    public DraftEmail Bcc(MailboxAddress mailboxAddress)
    {
        return AddRecipients(ref _bcc, mailboxAddress.ToAddressList());
    }

    public DraftEmail Bcc(IEnumerable<string> addresses)
    {
        return AddRecipients(ref _bcc, addresses.ToAddressList());
    }

    public DraftEmail Bcc(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        return AddRecipients(ref _bcc, mailboxAddresses.ToAddressList());
    }

    public DraftEmail Bcc(IList<MailboxAddress> mailboxAddresses)
    {
        return AddRecipients(ref _bcc, mailboxAddresses.ToAddressList());
    }

    public DraftEmail Subject(string subject)
    {
        if (subject is null)
            throw new ArgumentNullException(nameof(subject), "The subject cannot be null.");

        _subject.EnsureNotSet(nameof(Email.Subject));
        _templateId.EnsureNotSet(nameof(Email.TemplateId), nameof(Email.Subject), TemplateContentConflictGuidance);
        _templateAlias.EnsureNotSet(nameof(Email.TemplateAlias), nameof(Email.Subject), TemplateContentConflictGuidance);
        _templateModel.EnsureNotSet(nameof(Email.TemplateModel), nameof(Email.Subject), TemplateContentConflictGuidance);

        var length = subject.AsSpan()
            .GetPostmarkCharacterCount();
        if (length > 2000)
            throw new ArgumentException($"The subject cannot be longer than 2000 characters. Actual length: {length}.", nameof(subject));

        _subject = subject;

        return this;
    }

    public DraftEmail HtmlBody(string html)
    {
        if (html is null)
            throw new ArgumentNullException(nameof(html), "The HTML body cannot be null.");

        _templateId.EnsureNotSet(nameof(Email.TemplateId), nameof(Email.HtmlBody), TemplateContentConflictGuidance);
        _templateAlias.EnsureNotSet(nameof(Email.TemplateAlias), nameof(Email.HtmlBody), TemplateContentConflictGuidance);
        _templateModel.EnsureNotSet(nameof(Email.TemplateModel), nameof(Email.HtmlBody), TemplateContentConflictGuidance);
        _inlineCss.EnsureNotSet(nameof(Email.InlineCss), nameof(Email.HtmlBody), TemplateContentConflictGuidance);
        _htmlBody.EnsureNotSet(nameof(Email.HtmlBody));

        _htmlBody = html;

        return this;
    }

    public DraftEmail TextBody(string text)
    {
        if (text is null)
            throw new ArgumentNullException(nameof(text), "The text body cannot be null.");

        _templateId.EnsureNotSet(nameof(Email.TemplateId), nameof(Email.TextBody), TemplateContentConflictGuidance);
        _templateAlias.EnsureNotSet(nameof(Email.TemplateAlias), nameof(Email.TextBody), TemplateContentConflictGuidance);
        _templateModel.EnsureNotSet(nameof(Email.TemplateModel), nameof(Email.TextBody), TemplateContentConflictGuidance);
        _inlineCss.EnsureNotSet(nameof(Email.InlineCss), nameof(Email.TextBody), TemplateContentConflictGuidance);
        _textBody.EnsureNotSet(nameof(Email.TextBody));

        _textBody = text;

        return this;
    }

    public DraftEmail WithTag(string tag)
    {
        _tag.EnsureNotSet(nameof(Email.Tag));
        if (tag is null)
            throw new ArgumentNullException(nameof(tag), "The tag cannot be null.");

        if (tag.Length > 1000)
            throw new ArgumentException($"The tag cannot be longer than 1000 characters. Actual length: {tag.Length}.", nameof(tag));

        _tag = tag;

        return this;
    }

    public DraftEmail AddHeader(string name, string value)
    {
        ValidationExtensions.ValidateHeaderName(name, nameof(name));
        ValidationExtensions.ValidateHeaderValue(value, nameof(value));

        AddHeaderEntry(name, value, nameof(name));

        return this;
    }

    public DraftEmail AddHeader(KeyValuePair<string, string> header)
    {
        ValidationExtensions.ValidateHeader(header.Key, header.Value, nameof(header));

        AddHeaderEntry(header.Key, header.Value, nameof(header));

        return this;
    }

    public DraftEmail AddHeader(IEnumerable<KeyValuePair<string, string>> headers)
    {
        var headerList = ValidationExtensions.SnapshotValidatedHeaders(headers, nameof(headers), _headers);

        foreach (var header in headerList)
            AddHeaderEntry(header.Key, header.Value, nameof(headers));

        return this;
    }

    public DraftEmail AddHeader(IDictionary<string, string> headers)
    {
        var headerList = ValidationExtensions.SnapshotValidatedHeaders(headers, nameof(headers), _headers);

        foreach (var header in headerList)
            AddHeaderEntry(header.Key, header.Value, nameof(headers));

        return this;
    }

    public DraftEmail AddMetadata(string name, string value)
    {
        ValidationExtensions.ValidateMetadataName(name, nameof(name));
        ValidationExtensions.ValidateMetadataValue(value, nameof(value));

        AddMetadataEntry(name, value, nameof(name));

        return this;
    }

    public DraftEmail AddMetadata(KeyValuePair<string, string> entry)
    {
        ValidationExtensions.ValidateMetadata(entry.Key, entry.Value, nameof(entry));

        AddMetadataEntry(entry.Key, entry.Value, nameof(entry));

        return this;
    }

    public DraftEmail AddMetadata(IEnumerable<KeyValuePair<string, string>> metadata)
    {
        var metadataList = ValidationExtensions.SnapshotValidatedMetadata(metadata, nameof(metadata), _metadata);

        foreach (var entry in metadataList)
            AddMetadataEntry(entry.Key, entry.Value, nameof(metadata));

        return this;
    }

    public DraftEmail AddMetadata(IDictionary<string, string> metadata)
    {
        var metadataList = ValidationExtensions.SnapshotValidatedMetadata(metadata, nameof(metadata), _metadata);

        foreach (var entry in metadataList)
            AddMetadataEntry(entry.Key, entry.Value, nameof(metadata));

        return this;
    }

    public DraftEmail AddAttachment(Attachment attachment)
    {
        if (attachment is null)
            throw new ArgumentNullException(nameof(attachment), "The attachment cannot be null.");

        var estimatedSize = PostmarkSizeEstimator.EstimateBase64SizeLowerBound(attachment.Content);
        EnsureAttachmentsWithinLimit(estimatedSize);

        (_attachments ??= []).Add(attachment);

        return this;
    }

    public DraftEmail AddAttachment(IEnumerable<Attachment> attachments)
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

    public DraftEmail EnableOpenTracking()
    {
        _openTracking.EnsureNotSet(nameof(Email.OpenTracking));

        _openTracking = true;

        return this;
    }

    public DraftEmail UseLinkTracking(LinkTracking linkTracking = LinkTracking.HtmlAndText)
    {
        _linkTracking.EnsureNotSet(nameof(Email.LinkTracking));

        _linkTracking = linkTracking;

        return this;
    }

    public DraftEmail UseMessageStream(MessageStream messageStream)
    {
        _messageStream.EnsureNotSet(nameof(Email.MessageStream));

        _messageStream = messageStream switch
        {
            MessageStream.Transactional => "outbound",
            MessageStream.Broadcast => "broadcast",
            _ => throw new UnreachableException($"Enum value of '{nameof(MessageStream)}.{messageStream}' has not been handled.")
        };

        return this;
    }

    public DraftEmail UseMessageStream(string messageStreamId)
    {
        _messageStream.EnsureNotSet(nameof(Email.MessageStream));
        ValidationExtensions.ValidateMessageStreamId(messageStreamId, nameof(messageStreamId));

        _messageStream = messageStreamId;

        return this;
    }

    public DraftEmail SetTemplate(int templateId, bool? inlineCss = null)
    {
        _subject.EnsureNotSet(nameof(Email.Subject), nameof(Email.TemplateId), TemplateContentConflictGuidance);
        _htmlBody.EnsureNotSet(nameof(Email.HtmlBody), nameof(Email.TemplateId), TemplateContentConflictGuidance);
        _textBody.EnsureNotSet(nameof(Email.TextBody), nameof(Email.TemplateId), TemplateContentConflictGuidance);
        _templateId.EnsureNotSet(nameof(Email.TemplateId));
        _templateAlias.EnsureNotSet(nameof(Email.TemplateAlias), nameof(Email.TemplateId), "Only one template identifier can be set.");
        _inlineCss.EnsureNotSet(nameof(Email.InlineCss), nameof(Email.TemplateId));

        if (templateId <= 0)
            throw new ArgumentException($"The template ID must be greater than zero. Received {templateId}.", nameof(templateId));

        _templateId = templateId;
        _inlineCss = inlineCss;

        return this;
    }

    public DraftEmail SetTemplate(string templateAlias, bool? inlineCss = null)
    {
        _subject.EnsureNotSet(nameof(Email.Subject), nameof(Email.TemplateAlias), TemplateContentConflictGuidance);
        _htmlBody.EnsureNotSet(nameof(Email.HtmlBody), nameof(Email.TemplateAlias), TemplateContentConflictGuidance);
        _textBody.EnsureNotSet(nameof(Email.TextBody), nameof(Email.TemplateAlias), TemplateContentConflictGuidance);
        _templateId.EnsureNotSet(nameof(Email.TemplateId), nameof(Email.TemplateAlias), "Only one template identifier can be set.");
        _templateAlias.EnsureNotSet(nameof(Email.TemplateAlias));
        _inlineCss.EnsureNotSet(nameof(Email.InlineCss), nameof(Email.TemplateAlias));

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

    public DraftEmail WithModel(object templateModel)
    {
        return SetTemplateModel(templateModel);
    }

    public DraftEmail WithModel(object templateModel, JsonSerializerOptions serializerOptions)
    {
        if (serializerOptions is null)
            throw new ArgumentNullException(nameof(serializerOptions), "The serializer options cannot be null.");

        return SetTemplateModel(templateModel, serializerOptions);
    }

    public Email Build()
    {
        if (_from is null)
            throw new InvalidOperationException("From address is required before building the email. Call From(...).");

        var totalRecipients = (_to?.Count ?? 0) + (_cc?.Count ?? 0) + (_bcc?.Count ?? 0);
        if (totalRecipients == 0)
            throw new InvalidOperationException("At least one recipient is required before building the email. Call To(...), Cc(...), or Bcc(...).");

        if (totalRecipients > 50)
            throw new InvalidOperationException(
                $"There are too many recipients. Postmark implements a limit of 50 recipients per message. The recipient count includes all To, Cc, and Bcc recipients combined. Actual recipient count: {totalRecipients}.");

        if (_subject is null && !_templateId.HasValue && _templateAlias is null)
            throw new InvalidOperationException("Subject is required before building the email. Call Subject(...).");

        if (_textBody is null && _htmlBody is null && !_templateId.HasValue && _templateAlias is null)
            throw new InvalidOperationException("Message content is required before building the email. Call TextBody(...) or HtmlBody(...).");

        if ((_htmlBody is not null || _textBody is not null || _subject is not null) && (_templateId.HasValue || _templateAlias is not null))
            throw new InvalidOperationException(
                "Template emails cannot also set Subject, TextBody, or HtmlBody. Use either template fields (TemplateId or TemplateAlias with TemplateModel) or content fields (Subject with TextBody or HtmlBody).");

        if ((_templateId.HasValue || _templateAlias is not null) && _templateModel is null)
            throw new InvalidOperationException("TemplateModel is required when TemplateId or TemplateAlias is set. Call WithModel(...).");

        if (!_templateId.HasValue && _templateAlias is null && _templateModel is not null)
            throw new InvalidOperationException("TemplateId or TemplateAlias is required when TemplateModel is set. Call SetTemplate(...).");

        var textBodySize = PostmarkSizeEstimator.EstimateBodySizeLowerBound(_textBody);
        if (textBodySize > PostmarkSizeEstimator.BodySizeLimitInBytes)
            throw new InvalidOperationException(PostmarkSizeEstimator.FormatActualSizeLimitMessage("Text body exceeds Postmark's 5 MB limit.", textBodySize, PostmarkSizeEstimator.BodySizeLimitInBytes));

        var htmlBodySize = PostmarkSizeEstimator.EstimateBodySizeLowerBound(_htmlBody);
        if (htmlBodySize > PostmarkSizeEstimator.BodySizeLimitInBytes)
            throw new InvalidOperationException(PostmarkSizeEstimator.FormatActualSizeLimitMessage("HTML body exceeds Postmark's 5 MB limit.", htmlBodySize, PostmarkSizeEstimator.BodySizeLimitInBytes));

        EnsureMessageContentWithinLimit();

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
            TemplateModel = _templateModelSnapshot?.DeepClone(),
            TemplateModelNode = _templateModelSnapshot?.DeepClone(),
            TemplateModelSizeInBytes = _templateModelSizeInBytes,
            InlineCss = _inlineCss
        };

        return email;
    }

    private static void ValidateFrom(MailboxAddress mailboxAddress, string paramName)
    {
        var fromString = mailboxAddress.ToString(true);
        var length = fromString.AsSpan()
            .GetPostmarkCharacterCount();
        if (length > 255)
            throw new ArgumentException($"The {nameof(Email.From)} address cannot exceed 255 characters. Actual length: {length}.", paramName);
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

    private DraftEmail AddRecipients(ref IList<MailboxAddress>? recipients, IEnumerable<MailboxAddress> mailboxAddresses)
    {
        var snapshot = mailboxAddresses.ToAddressList();

        if (recipients is null)
            recipients = snapshot;
        else
            recipients.AddRange(snapshot);

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

        var projectedTotal = PostmarkSizeEstimator.EstimateMessageContentSizeLowerBound(_textBody, _htmlBody, _templateModelSizeInBytes, _attachments, _headers) + additionalBytes;
        if (projectedTotal > PostmarkSizeEstimator.MessageSizeLimitInBytes)
            throw new InvalidOperationException(PostmarkSizeEstimator.FormatEstimatedSizeLimitMessage("Estimated message content exceeds Postmark's 10 MB limit.", projectedTotal, PostmarkSizeEstimator.MessageSizeLimitInBytes));
    }

    private void EnsureMessageContentWithinLimit()
    {
        var projectedTotal = PostmarkSizeEstimator.EstimateMessageContentSizeLowerBound(_textBody, _htmlBody, _templateModelSizeInBytes, _attachments, _headers);
        if (projectedTotal > PostmarkSizeEstimator.MessageSizeLimitInBytes)
            throw new InvalidOperationException(PostmarkSizeEstimator.FormatEstimatedSizeLimitMessage("Estimated message content exceeds Postmark's 10 MB limit.", projectedTotal, PostmarkSizeEstimator.MessageSizeLimitInBytes));
    }

    private DraftEmail SetTemplateModel(object templateModel, JsonSerializerOptions? serializerOptions = null)
    {
        _templateModel.EnsureNotSet(nameof(Email.TemplateModel));

        var (snapshot, serializedSizeInBytes) = templateModel.SnapshotTemplateModel(nameof(templateModel), serializerOptions);

        _templateModel = templateModel;
        _templateModelSnapshot = snapshot;
        _templateModelSizeInBytes = serializedSizeInBytes;

        return this;
    }
}