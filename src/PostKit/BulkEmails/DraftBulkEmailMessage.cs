using System.Text.Json;
using System.Text.Json.Nodes;
using MimeKit;
using PostKit.Common;

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
        ValidationExtensions.EnsureAddressFirst(address, name);
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
        ValidationExtensions.EnsureAddressFirst(address, name);
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
        ValidationExtensions.EnsureAddressFirst(address, name);
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
        if (serializerOptions is null)
            throw new ArgumentNullException(nameof(serializerOptions), "The serializer options cannot be null.");

        return SetTemplateModel(templateModel, serializerOptions);
    }

    public DraftBulkEmailMessage AddMetadata(string name, string value)
    {
        ValidationExtensions.ValidateMetadataName(name, nameof(name));
        ValidationExtensions.ValidateMetadataValue(value, nameof(value));

        AddMetadataEntry(name, value, nameof(name));

        return this;
    }

    public DraftBulkEmailMessage AddMetadata(KeyValuePair<string, string> entry)
    {
        ValidationExtensions.ValidateMetadata(entry.Key, entry.Value, nameof(entry));

        AddMetadataEntry(entry.Key, entry.Value, nameof(entry));

        return this;
    }

    public DraftBulkEmailMessage AddMetadata(IEnumerable<KeyValuePair<string, string>> metadata)
    {
        var metadataList = ValidationExtensions.SnapshotValidatedMetadata(metadata, nameof(metadata), _metadata);

        foreach (var entry in metadataList)
            AddMetadataEntry(entry.Key, entry.Value, nameof(metadata));

        return this;
    }

    public DraftBulkEmailMessage AddMetadata(IDictionary<string, string> metadata)
    {
        var metadataList = ValidationExtensions.SnapshotValidatedMetadata(metadata, nameof(metadata), _metadata);

        foreach (var entry in metadataList)
            AddMetadataEntry(entry.Key, entry.Value, nameof(metadata));

        return this;
    }

    public DraftBulkEmailMessage AddHeader(string name, string value)
    {
        ValidationExtensions.ValidateHeaderName(name, nameof(name));
        ValidationExtensions.ValidateHeaderValue(value, nameof(value));

        AddHeaderEntry(name, value, nameof(name));

        return this;
    }

    public DraftBulkEmailMessage AddHeader(KeyValuePair<string, string> header)
    {
        ValidationExtensions.ValidateHeader(header.Key, header.Value, nameof(header));

        AddHeaderEntry(header.Key, header.Value, nameof(header));

        return this;
    }

    public DraftBulkEmailMessage AddHeader(IEnumerable<KeyValuePair<string, string>> headers)
    {
        var headerList = ValidationExtensions.SnapshotValidatedHeaders(headers, nameof(headers), _headers);

        foreach (var header in headerList)
            AddHeaderEntry(header.Key, header.Value, nameof(headers));

        return this;
    }

    public DraftBulkEmailMessage AddHeader(IDictionary<string, string> headers)
    {
        var headerList = ValidationExtensions.SnapshotValidatedHeaders(headers, nameof(headers), _headers);

        foreach (var header in headerList)
            AddHeaderEntry(header.Key, header.Value, nameof(headers));

        return this;
    }

    public BulkEmailMessage Build()
    {
        var totalRecipients = (_to?.Count ?? 0) + (_cc?.Count ?? 0) + (_bcc?.Count ?? 0);
        if (totalRecipients == 0)
            throw new InvalidOperationException("At least one recipient is required before building the bulk email message. Call To(...), Cc(...), or Bcc(...).");

        if (totalRecipients > 50)
            throw new InvalidOperationException($"There are too many recipients. Postmark implements a limit of 50 recipients per message. The recipient count includes all To, Cc, and Bcc recipients combined. Actual recipient count: {totalRecipients}.");

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
        if (_metadata is not null && ValidationExtensions.TryGetExistingKey(_metadata, name, out var existingName))
            throw new ArgumentException(ValidationExtensions.FormatDuplicateExistingMetadataMessage(name, existingName), paramName);

        var existingCount = _metadata?.Count ?? 0;
        if (existingCount >= 10)
            throw new InvalidOperationException($"Cannot set more than 10 metadata fields for a message. Adding 1 metadata field to the existing {existingCount} would produce {existingCount + 1}.");

        (_metadata ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)).Add(name, value);
    }

    private void AddHeaderEntry(string name, string value, string paramName)
    {
        if (_headers is not null && ValidationExtensions.TryGetExistingKey(_headers, name, out var existingName))
            throw new ArgumentException(ValidationExtensions.FormatDuplicateExistingHeaderMessage(name, existingName), paramName);

        (_headers ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)).Add(name, value);
    }

}
