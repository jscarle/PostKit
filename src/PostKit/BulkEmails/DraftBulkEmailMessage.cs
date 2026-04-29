using System.Text.Json;
using System.Text.Json.Nodes;
using MimeKit;
using PostKit.Postmark.Common;

namespace PostKit.BulkEmails;

internal sealed class DraftBulkEmailMessage
{
    private IList<MailboxAddress>? _to;
    private IList<MailboxAddress>? _cc;
    private IList<MailboxAddress>? _bcc;
    private object? _templateModel;
    private JsonNode? _templateModelSnapshot;
    private int _templateModelSizeInBytes;
    private Dictionary<string, string>? _metadata;
    private Dictionary<string, string>? _headers;

    public DraftBulkEmailMessage To(string address)
    {
        return AddRecipients(ref _to, address.ToAddressList());
    }

    public DraftBulkEmailMessage To(string address, string? name)
    {
        return AddRecipients(ref _to, (address, name).ToAddressList());
    }

    public DraftBulkEmailMessage To(MailboxAddress mailboxAddress)
    {
        return AddRecipients(ref _to, mailboxAddress.ToAddressList());
    }

    public DraftBulkEmailMessage To(IEnumerable<string> addresses)
    {
        return AddRecipients(ref _to, addresses.ToAddressList());
    }

    public DraftBulkEmailMessage To(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        return AddRecipients(ref _to, mailboxAddresses.ToAddressList());
    }

    public DraftBulkEmailMessage To(IList<MailboxAddress> mailboxAddresses)
    {
        return AddRecipients(ref _to, mailboxAddresses.ToAddressList());
    }

    public DraftBulkEmailMessage Cc(string address)
    {
        return AddRecipients(ref _cc, address.ToAddressList());
    }

    public DraftBulkEmailMessage Cc(string address, string? name)
    {
        return AddRecipients(ref _cc, (address, name).ToAddressList());
    }

    public DraftBulkEmailMessage Cc(MailboxAddress mailboxAddress)
    {
        return AddRecipients(ref _cc, mailboxAddress.ToAddressList());
    }

    public DraftBulkEmailMessage Cc(IEnumerable<string> addresses)
    {
        return AddRecipients(ref _cc, addresses.ToAddressList());
    }

    public DraftBulkEmailMessage Cc(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        return AddRecipients(ref _cc, mailboxAddresses.ToAddressList());
    }

    public DraftBulkEmailMessage Cc(IList<MailboxAddress> mailboxAddresses)
    {
        return AddRecipients(ref _cc, mailboxAddresses.ToAddressList());
    }

    public DraftBulkEmailMessage Bcc(string address)
    {
        return AddRecipients(ref _bcc, address.ToAddressList());
    }

    public DraftBulkEmailMessage Bcc(string address, string? name)
    {
        return AddRecipients(ref _bcc, (address, name).ToAddressList());
    }

    public DraftBulkEmailMessage Bcc(MailboxAddress mailboxAddress)
    {
        return AddRecipients(ref _bcc, mailboxAddress.ToAddressList());
    }

    public DraftBulkEmailMessage Bcc(IEnumerable<string> addresses)
    {
        return AddRecipients(ref _bcc, addresses.ToAddressList());
    }

    public DraftBulkEmailMessage Bcc(IEnumerable<MailboxAddress> mailboxAddresses)
    {
        return AddRecipients(ref _bcc, mailboxAddresses.ToAddressList());
    }

    public DraftBulkEmailMessage Bcc(IList<MailboxAddress> mailboxAddresses)
    {
        return AddRecipients(ref _bcc, mailboxAddresses.ToAddressList());
    }

    public DraftBulkEmailMessage WithModel(object templateModel)
    {
        return SetTemplateModel(templateModel);
    }

    public DraftBulkEmailMessage WithModel(object templateModel, JsonSerializerOptions serializerOptions)
    {
        ArgumentNullException.ThrowIfNull(serializerOptions);
        return SetTemplateModel(templateModel, serializerOptions);
    }

    public DraftBulkEmailMessage AddMetadata(string name, string value)
    {
        ValidateMetadataName(name, nameof(name));
        ValidateMetadataValue(value, nameof(value));

        AddMetadataEntry(name, value, nameof(name));

        return this;
    }

    public DraftBulkEmailMessage AddMetadata(KeyValuePair<string, string> entry)
    {
        ValidateMetadata(entry.Key, entry.Value, nameof(entry));

        AddMetadataEntry(entry.Key, entry.Value, nameof(entry));

        return this;
    }

    public DraftBulkEmailMessage AddMetadata(IEnumerable<KeyValuePair<string, string>> metadata)
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

    public DraftBulkEmailMessage AddMetadata(IDictionary<string, string> metadata)
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

    public DraftBulkEmailMessage AddHeader(string name, string value)
    {
        ValidateHeaderName(name, nameof(name));
        ValidateHeaderValue(value, nameof(value));

        AddHeaderEntry(name, value, nameof(name));

        return this;
    }

    public DraftBulkEmailMessage AddHeader(KeyValuePair<string, string> header)
    {
        ValidateHeader(header.Key, header.Value, nameof(header));

        AddHeaderEntry(header.Key, header.Value, nameof(header));

        return this;
    }

    public DraftBulkEmailMessage AddHeader(IEnumerable<KeyValuePair<string, string>> headers)
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

    public DraftBulkEmailMessage AddHeader(IDictionary<string, string> headers)
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

    public BulkEmailMessage Build()
    {
        var totalRecipients = (_to?.Count ?? 0) + (_cc?.Count ?? 0) + (_bcc?.Count ?? 0);
        if (totalRecipients == 0)
            throw new InvalidOperationException("At least one recipient is required.");

        if (totalRecipients > 50)
            throw new InvalidOperationException("There are too many recipients. Postmark implements a limit of 50 recipients per message. The recipient count includes all To, Cc, and Bcc recipients combined.");

        return new BulkEmailMessage
        {
            To = _to?.SnapshotReadOnly(),
            Cc = _cc?.SnapshotReadOnly(),
            Bcc = _bcc?.SnapshotReadOnly(),
            TemplateModel = _templateModelSnapshot?.DeepClone(),
            TemplateModelNode = _templateModelSnapshot?.DeepClone(),
            TemplateModelSizeInBytes = _templateModelSizeInBytes,
            Metadata = _metadata?.SnapshotReadOnly(),
            Headers = _headers?.SnapshotReadOnly(),
        };
    }

    private DraftBulkEmailMessage AddRecipients(ref IList<MailboxAddress>? recipients, IEnumerable<MailboxAddress> mailboxAddresses)
    {
        var snapshot = mailboxAddresses.ToAddressList();

        if (recipients is null)
            recipients = snapshot;
        else
            recipients.AddRange(snapshot);

        return this;
    }

    private DraftBulkEmailMessage SetTemplateModel(object templateModel, JsonSerializerOptions? serializerOptions = null)
    {
        _templateModel.EnsureNotSet(nameof(BulkEmailMessage.TemplateModel));

        var (snapshot, serializedSizeInBytes) = templateModel.SnapshotTemplateModel(nameof(templateModel), serializerOptions);

        _templateModel = templateModel;
        _templateModelSnapshot = snapshot;
        _templateModelSizeInBytes = serializedSizeInBytes;

        return this;
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
}
