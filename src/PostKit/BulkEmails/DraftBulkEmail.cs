using System.Diagnostics;
using System.Text.RegularExpressions;
using MimeKit;
using PostKit.Common;
using PostKit.Postmark.Common;

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
    private long _attachmentBytes;
    private int? _templateId;
    private string? _templateAlias;
    private bool? _inlineCss;
    private readonly List<BulkEmailMessage> _messages = [];

    public DraftBulkEmail From(string address)
    {
        _from.EnsureNotSet(nameof(BulkEmail.From));

        var mailboxAddress = MailboxAddress.Parse(address);

        ValidateFrom(mailboxAddress, nameof(address));

        _from = mailboxAddress;

        return this;
    }

    public DraftBulkEmail From(string? name, string address)
    {
        _from.EnsureNotSet(nameof(BulkEmail.From));

        var mailboxAddress = new MailboxAddress(name, address);

        ValidateFrom(mailboxAddress, nameof(address));

        _from = mailboxAddress;

        return this;
    }

    public DraftBulkEmail From(MailboxAddress mailboxAddress)
    {
        ArgumentNullException.ThrowIfNull(mailboxAddress);

        _from.EnsureNotSet(nameof(BulkEmail.From));

        ValidateFrom(mailboxAddress, nameof(mailboxAddress));

        _from = mailboxAddress;

        return this;
    }

    public DraftBulkEmail ReplyTo(string address)
    {
        return AddReplyTo(address.ToAddressList());
    }

    public DraftBulkEmail ReplyTo(string? name, string address)
    {
        return AddReplyTo((name, address).ToAddressList());
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
        ArgumentNullException.ThrowIfNull(subject);

        _subject.EnsureNotSet(nameof(BulkEmail.Subject));
        _templateId.EnsureNotSet(nameof(BulkEmail.TemplateId));
        _templateAlias.EnsureNotSet(nameof(BulkEmail.TemplateAlias));

        var length = subject.AsSpan()
            .GetPostmarkCharacterCount();
        if (length > 2000)
            throw new ArgumentException("The subject cannot be longer than 2000 characters.", nameof(subject));

        _subject = subject;

        return this;
    }

    public DraftBulkEmail HtmlBody(string htmlBody)
    {
        ArgumentNullException.ThrowIfNull(htmlBody);

        _templateId.EnsureNotSet(nameof(BulkEmail.TemplateId));
        _templateAlias.EnsureNotSet(nameof(BulkEmail.TemplateAlias));
        _inlineCss.EnsureNotSet(nameof(BulkEmail.InlineCss));
        _htmlBody.EnsureNotSet(nameof(BulkEmail.HtmlBody));

        _htmlBody = htmlBody;

        return this;
    }

    public DraftBulkEmail TextBody(string textBody)
    {
        ArgumentNullException.ThrowIfNull(textBody);

        _templateId.EnsureNotSet(nameof(BulkEmail.TemplateId));
        _templateAlias.EnsureNotSet(nameof(BulkEmail.TemplateAlias));
        _inlineCss.EnsureNotSet(nameof(BulkEmail.InlineCss));
        _textBody.EnsureNotSet(nameof(BulkEmail.TextBody));

        _textBody = textBody;

        return this;
    }

    public DraftBulkEmail WithTag(string tag)
    {
        _tag.EnsureNotSet(nameof(BulkEmail.Tag));
        ArgumentNullException.ThrowIfNull(tag);

        if (tag.Length > 1000)
            throw new ArgumentException("The tag cannot be longer than 1000 characters.", nameof(tag));

        _tag = tag;

        return this;
    }

    public DraftBulkEmail AddHeader(string name, string value)
    {
        ValidateHeaderName(name, nameof(name));
        ValidateHeaderValue(value, nameof(value));

        AddHeaderEntry(name, value, nameof(name));

        return this;
    }

    public DraftBulkEmail AddHeader(KeyValuePair<string, string> header)
    {
        ValidateHeader(header.Key, header.Value, nameof(header));

        AddHeaderEntry(header.Key, header.Value, nameof(header));

        return this;
    }

    public DraftBulkEmail AddHeader(IEnumerable<KeyValuePair<string, string>> headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        var headerList = headers.ToList();
        var uniqueKeys = headerList.Select(static header => header.Key)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        if (uniqueKeys != headerList.Count)
            throw new ArgumentException("There are duplicate header entries.", nameof(headers));

        foreach (var header in headerList)
        {
            ValidateHeader(header.Key, header.Value, nameof(headers));
            AddHeaderEntry(header.Key, header.Value, nameof(headers));
        }

        return this;
    }

    public DraftBulkEmail AddHeader(IDictionary<string, string> headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        var uniqueKeys = headers.Keys
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        if (uniqueKeys != headers.Keys.Count)
            throw new ArgumentException("There are duplicate header entries.", nameof(headers));

        foreach (var header in headers)
        {
            ValidateHeader(header.Key, header.Value, nameof(headers));
            AddHeaderEntry(header.Key, header.Value, nameof(headers));
        }

        return this;
    }

    public DraftBulkEmail AddMetadata(string name, string value)
    {
        ValidateMetadataName(name, nameof(name));
        ValidateMetadataValue(value, nameof(value));

        AddMetadataEntry(name, value, nameof(name));

        return this;
    }

    public DraftBulkEmail AddMetadata(KeyValuePair<string, string> entry)
    {
        ValidateMetadata(entry.Key, entry.Value, nameof(entry));

        AddMetadataEntry(entry.Key, entry.Value, nameof(entry));

        return this;
    }

    public DraftBulkEmail AddMetadata(IEnumerable<KeyValuePair<string, string>> metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var metadataList = metadata.ToList();
        var uniqueKeys = metadataList.Select(static entry => entry.Key)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        if (uniqueKeys != metadataList.Count)
            throw new ArgumentException("There are duplicate metadata entries.", nameof(metadata));

        var projectedCount = (_metadata?.Count ?? 0) + metadataList.Count;
        if (projectedCount > 10)
            throw new ArgumentException("Cannot set more than 10 metadata values.", nameof(metadata));

        foreach (var entry in metadataList)
        {
            ValidateMetadata(entry.Key, entry.Value, nameof(metadata));
            AddMetadataEntry(entry.Key, entry.Value, nameof(metadata));
        }

        return this;
    }

    public DraftBulkEmail AddMetadata(IDictionary<string, string> metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var uniqueKeys = metadata.Keys
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        if (uniqueKeys != metadata.Keys.Count)
            throw new ArgumentException("There are duplicate metadata entries.", nameof(metadata));

        var projectedCount = (_metadata?.Count ?? 0) + metadata.Count;
        if (projectedCount > 10)
            throw new ArgumentException("Cannot set more than 10 metadata values.", nameof(metadata));

        foreach (var entry in metadata)
        {
            ValidateMetadata(entry.Key, entry.Value, nameof(metadata));
            AddMetadataEntry(entry.Key, entry.Value, nameof(metadata));
        }

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
            MessageStream.Transactional => throw new ArgumentException("The Bulk API only supports broadcast message streams.", nameof(messageStream)),
            _ => throw new UnreachableException($"Enum value of '{nameof(MessageStream)}.{messageStream}' has not been handled."),
        };

        return this;
    }

    public DraftBulkEmail UseMessageStream(string messageStreamId)
    {
        _messageStream.EnsureNotSet(nameof(BulkEmail.MessageStream));
        ArgumentNullException.ThrowIfNull(messageStreamId);

        if (!messageStreamId.AsSpan().IsValidMessageStreamId())
            throw new ArgumentException("The message stream ID is invalid.", nameof(messageStreamId));

        if (string.Equals(messageStreamId, "outbound", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The Bulk API only supports broadcast message streams.", nameof(messageStreamId));

        _messageStream = messageStreamId;

        return this;
    }

    public DraftBulkEmail AddAttachment(Attachment attachment)
    {
        ArgumentNullException.ThrowIfNull(attachment);

        var estimatedSize = PostmarkSizeEstimator.EstimateBase64SizeLowerBound(attachment.Content);
        EnsureAttachmentsWithinLimit(estimatedSize);

        (_attachments ??= []).Add(attachment);
        _attachmentBytes += estimatedSize;

        return this;
    }

    public DraftBulkEmail AddAttachment(IEnumerable<Attachment> attachments)
    {
        ArgumentNullException.ThrowIfNull(attachments);

        var buffer = attachments as ICollection<Attachment> ?? attachments.ToList();
        if (buffer.Count == 0)
            return this;

        long additionalBytes = 0;
        foreach (var attachment in buffer)
        {
            ArgumentNullException.ThrowIfNull(attachment);
            additionalBytes += PostmarkSizeEstimator.EstimateBase64SizeLowerBound(attachment.Content);
        }

        EnsureAttachmentsWithinLimit(additionalBytes);

        (_attachments ??= []).AddRange(buffer);
        _attachmentBytes += additionalBytes;

        return this;
    }

    public DraftBulkEmail AddMessage(BulkEmailMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        _messages.Add(message);

        return this;
    }

    public DraftBulkEmail AddMessage(IEnumerable<BulkEmailMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);

        foreach (var message in messages)
        {
            ArgumentNullException.ThrowIfNull(message);
            _messages.Add(message);
        }

        return this;
    }

    public DraftBulkEmail SetTemplate(int templateId, bool? inlineCss = null)
    {
        _subject.EnsureNotSet(nameof(BulkEmail.Subject));
        _htmlBody.EnsureNotSet(nameof(BulkEmail.HtmlBody));
        _textBody.EnsureNotSet(nameof(BulkEmail.TextBody));
        _templateId.EnsureNotSet(nameof(BulkEmail.TemplateId));
        _templateAlias.EnsureNotSet(nameof(BulkEmail.TemplateAlias));
        _inlineCss.EnsureNotSet(nameof(BulkEmail.InlineCss));

        if (templateId <= 0)
            throw new ArgumentException("The template ID must be greater than zero.", nameof(templateId));

        _templateId = templateId;
        _inlineCss = inlineCss;

        return this;
    }

    public DraftBulkEmail SetTemplate(string templateAlias, bool? inlineCss = null)
    {
        _subject.EnsureNotSet(nameof(BulkEmail.Subject));
        _htmlBody.EnsureNotSet(nameof(BulkEmail.HtmlBody));
        _textBody.EnsureNotSet(nameof(BulkEmail.TextBody));
        _templateId.EnsureNotSet(nameof(BulkEmail.TemplateId));
        _templateAlias.EnsureNotSet(nameof(BulkEmail.TemplateAlias));
        _inlineCss.EnsureNotSet(nameof(BulkEmail.InlineCss));

        if (string.IsNullOrWhiteSpace(templateAlias))
            throw new ArgumentException("The template alias is required.", nameof(templateAlias));

        if (templateAlias.Length > TemplateAliasMaxLength)
            throw new ArgumentException($"The template alias must not exceed {TemplateAliasMaxLength} characters.", nameof(templateAlias));
#if NET9_0_OR_GREATER
        if (!TemplateAliasRegex.IsMatch(templateAlias))
#else
        if (!TemplateAliasRegex().IsMatch(templateAlias))
#endif
            throw new ArgumentException("The template alias must start with a letter and may only contain letters, numbers, '-', '_', or '.' characters.", nameof(templateAlias));

        _templateAlias = templateAlias;
        _inlineCss = inlineCss;

        return this;
    }

    public BulkEmail Build()
    {
        if (_from is null)
            throw new InvalidOperationException("From address is required.");

        if (_messages.Count == 0)
            throw new InvalidOperationException("At least one message is required.");

        if (_subject is null && !_templateId.HasValue && _templateAlias is null)
            throw new InvalidOperationException("Either a subject, or a template ID or alias, is required.");

        if (_textBody is null && _htmlBody is null && !_templateId.HasValue && _templateAlias is null)
            throw new InvalidOperationException("Either a text or HTML body, or a template ID or alias, is required.");

        if ((_htmlBody is not null || _textBody is not null || _subject is not null) && (_templateId.HasValue || _templateAlias is not null))
            throw new InvalidOperationException("Neither a text or HTML body, nor a subject may be specified when using a template.");

        var textBodySize = PostmarkSizeEstimator.EstimateBodySizeLowerBound(_textBody);
        if (textBodySize > PostmarkSizeEstimator.BodySizeLimitInBytes)
            throw new InvalidOperationException("Text body exceeds Postmark's 5 MB limit.");

        var htmlBodySize = PostmarkSizeEstimator.EstimateBodySizeLowerBound(_htmlBody);
        if (htmlBodySize > PostmarkSizeEstimator.BodySizeLimitInBytes)
            throw new InvalidOperationException("HTML body exceeds Postmark's 5 MB limit.");

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
                .AsReadOnly(),
        };

        var estimatedTotal = PostmarkSizeEstimator.EstimateBulkEmailPayloadSizeLowerBound(bulkEmail);
        if (estimatedTotal > PostmarkSizeEstimator.BulkPayloadSizeLimitInBytes)
            throw new InvalidOperationException("Estimated bulk request size exceeds Postmark's 50 MB limit.");

        return bulkEmail;
    }

    private static void ValidateFrom(MailboxAddress mailboxAddress, string paramName)
    {
        var fromString = mailboxAddress.ToString(true);
        var length = fromString.AsSpan()
            .GetPostmarkCharacterCount();
        if (length > 255)
            throw new ArgumentException($"The {nameof(BulkEmail.From)} address cannot exceed 255 characters.", paramName);
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
        if (_headers?.ContainsKey(name) == true)
            throw new ArgumentException("There are duplicate header entries.", paramName);

        (_headers ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)).Add(name, value);
    }

    private static void ValidateHeader(ReadOnlySpan<char> name, ReadOnlySpan<char> value, string paramName)
    {
        ValidateHeaderName(name, paramName);
        ValidateHeaderValue(value, paramName);
    }

    private static void ValidateHeaderName(ReadOnlySpan<char> name, string paramName)
    {
        if (!name.IsValidHeaderName())
            throw new ArgumentException("The header name is invalid.", paramName);
    }

    private static void ValidateHeaderValue(ReadOnlySpan<char> value, string paramName)
    {
        if (!value.IsValidHeaderValue())
            throw new ArgumentException("The header value is invalid.", paramName);
    }

    private void AddMetadataEntry(string name, string value, string paramName)
    {
        if (_metadata?.Count >= 10)
            throw new InvalidOperationException("Cannot add more than 10 metadata values.");

        if (_metadata?.ContainsKey(name) == true)
            throw new ArgumentException("There are duplicate metadata entries.", paramName);

        (_metadata ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)).Add(name, value);
    }

    private static void ValidateMetadata(ReadOnlySpan<char> name, ReadOnlySpan<char> value, string paramName)
    {
        ValidateMetadataName(name, paramName);
        ValidateMetadataValue(value, paramName);
    }

    private static void ValidateMetadataName(ReadOnlySpan<char> name, string paramName)
    {
        if (!IsValidMetadataName(name))
            throw new ArgumentException("The metadata name is invalid.", paramName);
    }

    private static void ValidateMetadataValue(ReadOnlySpan<char> value, string paramName)
    {
        if (!IsValidMetadataValue(value))
            throw new ArgumentException("The metadata value is invalid.", paramName);
    }

    private static bool IsValidMetadataName(ReadOnlySpan<char> name)
    {
        if (name.Length is 0 or > 20)
            return false;

        if (char.IsWhiteSpace(name[0]) || char.IsWhiteSpace(name[^1]))
            return false;

        return true;
    }

    private static bool IsValidMetadataValue(ReadOnlySpan<char> value)
    {
        return value.Length <= 80;
    }

    private void EnsureAttachmentsWithinLimit(long additionalBytes)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(additionalBytes);

        var projectedTotal = PostmarkSizeEstimator.EstimateMessageContentSizeLowerBound(_textBody, _htmlBody, templateModelSizeInBytes: 0, _attachments, _headers)
            + additionalBytes;
        if (projectedTotal > PostmarkSizeEstimator.MessageSizeLimitInBytes)
            throw new InvalidOperationException("Estimated message content exceeds Postmark's 10 MB limit.");
    }

    private void EnsureMessageContentWithinLimit()
    {
        var sharedContentSize = PostmarkSizeEstimator.EstimateMessageContentSizeLowerBound(_textBody, _htmlBody, templateModelSizeInBytes: 0, _attachments, _headers);
        if (sharedContentSize > PostmarkSizeEstimator.MessageSizeLimitInBytes)
            throw new InvalidOperationException("Estimated message content exceeds Postmark's 10 MB limit.");

        foreach (var message in _messages)
        {
            var projectedTotal = sharedContentSize
                                 + message.TemplateModelSizeInBytes
                                 + PostmarkSizeEstimator.EstimateHeaderSizeLowerBound(message.Headers);
            if (projectedTotal > PostmarkSizeEstimator.MessageSizeLimitInBytes)
                throw new InvalidOperationException("Estimated message content exceeds Postmark's 10 MB limit.");
        }
    }
}
