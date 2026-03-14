using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using MimeKit;
using PostKit.Common;
using PostKit.Postmark.Common;

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
    private long _attachmentBytes;
    private int? _templateId;
    private string? _templateAlias;
    private object? _templateModel;
    private JsonNode? _templateModelSnapshot;
    private int _templateModelSizeInBytes;
    private bool? _inlineCss;

    public DraftEmail From(string address)
    {
        _from.EnsureNotSet(nameof(Email.From));

        var mailboxAddress = MailboxAddress.Parse(address);

        ValidateFrom(mailboxAddress, nameof(address));

        _from = mailboxAddress;

        return this;
    }

    public DraftEmail From(string? name, string address)
    {
        _from.EnsureNotSet(nameof(Email.From));

        var mailboxAddress = new MailboxAddress(name, address);

        ValidateFrom(mailboxAddress, nameof(address));

        _from = mailboxAddress;

        return this;
    }

    public DraftEmail From(MailboxAddress mailboxAddress)
    {
        ArgumentNullException.ThrowIfNull(mailboxAddress);

        _from.EnsureNotSet(nameof(Email.From));

        ValidateFrom(mailboxAddress, nameof(mailboxAddress));

        _from = mailboxAddress;

        return this;
    }

    public DraftEmail ReplyTo(string address)
    {
        return AddRecipients(ref _replyTo, address.ToAddressList());
    }

    public DraftEmail ReplyTo(string? name, string address)
    {
        return AddRecipients(ref _replyTo, (name, address).ToAddressList());
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

    public DraftEmail To(string? name, string address)
    {
        return AddRecipients(ref _to, (name, address).ToAddressList());
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

    public DraftEmail Cc(string? name, string address)
    {
        return AddRecipients(ref _cc, (name, address).ToAddressList());
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

    public DraftEmail Bcc(string? name, string address)
    {
        return AddRecipients(ref _bcc, (name, address).ToAddressList());
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
        ArgumentNullException.ThrowIfNull(subject);

        _subject.EnsureNotSet(nameof(Email.Subject));
        _templateId.EnsureNotSet(nameof(Email.TemplateId));
        _templateAlias.EnsureNotSet(nameof(Email.TemplateAlias));
        _templateModel.EnsureNotSet(nameof(Email.TemplateModel));

        var length = subject.AsSpan()
            .GetPostmarkCharacterCount();
        if (length > 2000)
            throw new ArgumentException("The subject cannot be longer than 2000 characters.", nameof(subject));

        _subject = subject;

        return this;
    }

    public DraftEmail HtmlBody(string html)
    {
        ArgumentNullException.ThrowIfNull(html);

        _templateId.EnsureNotSet(nameof(Email.TemplateId));
        _templateAlias.EnsureNotSet(nameof(Email.TemplateAlias));
        _templateModel.EnsureNotSet(nameof(Email.TemplateModel));
        _inlineCss.EnsureNotSet(nameof(Email.InlineCss));
        _htmlBody.EnsureNotSet(nameof(Email.HtmlBody));

        _htmlBody = html;

        return this;
    }

    public DraftEmail TextBody(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        _templateId.EnsureNotSet(nameof(Email.TemplateId));
        _templateAlias.EnsureNotSet(nameof(Email.TemplateAlias));
        _templateModel.EnsureNotSet(nameof(Email.TemplateModel));
        _inlineCss.EnsureNotSet(nameof(Email.InlineCss));
        _textBody.EnsureNotSet(nameof(Email.TextBody));

        _textBody = text;

        return this;
    }

    public DraftEmail WithTag(string tag)
    {
        _tag.EnsureNotSet(nameof(Email.Tag));
        ArgumentNullException.ThrowIfNull(tag);

        if (tag.Length > 1000)
            throw new ArgumentException("The tag cannot be longer than 1000 characters.", nameof(tag));

        _tag = tag;

        return this;
    }

    public DraftEmail AddHeader(string name, string value)
    {
        ValidateHeaderName(name, nameof(name));
        ValidateHeaderValue(value, nameof(value));

        AddHeaderEntry(name, value, nameof(name));

        return this;
    }

    public DraftEmail AddHeader(KeyValuePair<string, string> header)
    {
        ValidateHeader(header.Key, header.Value, nameof(header));

        AddHeaderEntry(header.Key, header.Value, nameof(header));

        return this;
    }

    public DraftEmail AddHeader(IEnumerable<KeyValuePair<string, string>> headers)
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

    public DraftEmail AddHeader(IDictionary<string, string> headers)
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

    public DraftEmail AddMetadata(string name, string value)
    {
        ValidateMetadataName(name, nameof(name));
        ValidateMetadataValue(value, nameof(value));

        AddMetadataEntry(name, value, nameof(name));

        return this;
    }

    public DraftEmail AddMetadata(KeyValuePair<string, string> entry)
    {
        ValidateMetadata(entry.Key, entry.Value, nameof(entry));

        AddMetadataEntry(entry.Key, entry.Value, nameof(entry));

        return this;
    }

    public DraftEmail AddMetadata(IEnumerable<KeyValuePair<string, string>> metadata)
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

    public DraftEmail AddMetadata(IDictionary<string, string> metadata)
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

    public DraftEmail AddAttachment(Attachment attachment)
    {
        ArgumentNullException.ThrowIfNull(attachment);

        var estimatedSize = PostmarkSizeEstimator.EstimateBase64SizeLowerBound(attachment.Content);
        EnsureAttachmentsWithinLimit(estimatedSize);

        (_attachments ??= []).Add(attachment);
        _attachmentBytes += estimatedSize;

        return this;
    }

    public DraftEmail AddAttachment(IEnumerable<Attachment> attachments)
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
            _ => throw new UnreachableException($"Enum value of '{nameof(MessageStream)}.{messageStream}' has not been handled."),
        };

        return this;
    }

    public DraftEmail UseMessageStream(string messageStreamId)
    {
        _messageStream.EnsureNotSet(nameof(Email.MessageStream));

        if (!messageStreamId.AsSpan().IsValidMessageStreamId())
            throw new ArgumentException("The message stream ID is invalid.", nameof(messageStreamId));

        _messageStream = messageStreamId;

        return this;
    }

    public DraftEmail SetTemplate(int templateId, bool? inlineCss = null)
    {
        _subject.EnsureNotSet(nameof(Email.Subject));
        _htmlBody.EnsureNotSet(nameof(Email.HtmlBody));
        _textBody.EnsureNotSet(nameof(Email.TextBody));
        _templateId.EnsureNotSet(nameof(Email.TemplateId));
        _templateAlias.EnsureNotSet(nameof(Email.TemplateAlias));
        _inlineCss.EnsureNotSet(nameof(Email.InlineCss));

        if (templateId <= 0)
            throw new ArgumentException("The template ID must be greater than zero.", nameof(templateId));

        _templateId = templateId;
        _inlineCss = inlineCss;

        return this;
    }

    public DraftEmail SetTemplate(string templateAlias, bool? inlineCss = null)
    {
        _subject.EnsureNotSet(nameof(Email.Subject));
        _htmlBody.EnsureNotSet(nameof(Email.HtmlBody));
        _textBody.EnsureNotSet(nameof(Email.TextBody));
        _templateId.EnsureNotSet(nameof(Email.TemplateId));
        _templateAlias.EnsureNotSet(nameof(Email.TemplateAlias));
        _inlineCss.EnsureNotSet(nameof(Email.InlineCss));

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

    public DraftEmail WithModel(object templateModel)
    {
        return SetTemplateModel(templateModel);
    }

    public DraftEmail WithModel(object templateModel, JsonSerializerOptions serializerOptions)
    {
        ArgumentNullException.ThrowIfNull(serializerOptions);
        return SetTemplateModel(templateModel, serializerOptions);
    }

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

        if ((_htmlBody is not null || _textBody is not null || _subject is not null) && (_templateId.HasValue || _templateAlias is not null))
            throw new InvalidOperationException("Neither a text or HTML body, nor a subject may be specified when using a template.");

        if ((_templateId.HasValue || _templateAlias is not null) && _templateModel is null)
            throw new InvalidOperationException("A template model is required when using a template.");

        if (!_templateId.HasValue && _templateAlias is null && _templateModel is not null)
            throw new InvalidOperationException("A template ID or alias is required when using a template model.");

        var textBodySize = PostmarkSizeEstimator.EstimateBodySizeLowerBound(_textBody);
        if (textBodySize > PostmarkSizeEstimator.BodySizeLimitInBytes)
            throw new InvalidOperationException("Text body exceeds Postmark's 5 MB limit.");

        var htmlBodySize = PostmarkSizeEstimator.EstimateBodySizeLowerBound(_htmlBody);
        if (htmlBodySize > PostmarkSizeEstimator.BodySizeLimitInBytes)
            throw new InvalidOperationException("HTML body exceeds Postmark's 5 MB limit.");

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
            TemplateModel = _templateModel,
            TemplateModelNode = _templateModelSnapshot?.DeepClone(),
            TemplateModelSizeInBytes = _templateModelSizeInBytes,
            InlineCss = _inlineCss,
        };

        return email;
    }

    private static void ValidateFrom(MailboxAddress mailboxAddress, string paramName)
    {
        var fromString = mailboxAddress.ToString(true);
        var length = fromString.AsSpan()
            .GetPostmarkCharacterCount();
        if (length > 255)
            throw new ArgumentException($"The {nameof(Email.From)} address cannot exceed 255 characters.", paramName);
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
        return value.Length is > 0 and <= 80;
    }

    private void EnsureAttachmentsWithinLimit(long additionalBytes)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(additionalBytes);

        var projectedTotal = PostmarkSizeEstimator.EstimateMessageContentSizeLowerBound(_textBody, _htmlBody, _templateModelSizeInBytes, _attachments)
            + additionalBytes;
        if (projectedTotal > PostmarkSizeEstimator.MessageSizeLimitInBytes)
            throw new InvalidOperationException("Estimated message content exceeds Postmark's 10 MB limit.");
    }

    private void EnsureMessageContentWithinLimit()
    {
        var projectedTotal = PostmarkSizeEstimator.EstimateMessageContentSizeLowerBound(_textBody, _htmlBody, _templateModelSizeInBytes, _attachments);
        if (projectedTotal > PostmarkSizeEstimator.MessageSizeLimitInBytes)
            throw new InvalidOperationException("Estimated message content exceeds Postmark's 10 MB limit.");
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
