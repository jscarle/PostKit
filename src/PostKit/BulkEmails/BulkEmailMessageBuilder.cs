// ReSharper disable RedundantExtendsListEntry
// Intentional reference as the code is separated into multiple files.

using PostKit.Postmark.Common;

namespace PostKit.BulkEmails;

/// <summary>Provides a fluent interface for constructing <see cref="BulkEmailMessage"/> values.</summary>
public sealed partial class BulkEmailMessageBuilder : IBulkEmailMessageBuilder
{
    internal BulkEmailMessageBuilder()
    {
    }

    /// <inheritdoc/>
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
            TemplateModel = _templateModel,
            TemplateModelNode = _templateModelSnapshot?.DeepClone(),
            TemplateModelSizeInBytes = _templateModelSizeInBytes,
            Metadata = _metadata?.SnapshotReadOnly(),
            Headers = _headers?.SnapshotReadOnly(),
        };
    }
}
