using System.Text.Json.Nodes;
using PostKit.Postmark.Common;

namespace PostKit.BulkEmails;

partial class BulkEmailMessageBuilder
{
    private object? _templateModel;
    private JsonNode? _templateModelSnapshot;
    private int _templateModelSizeInBytes;

    /// <inheritdoc/>
    public IBulkEmailMessageBuilder WithTemplateModel(object templateModel)
    {
        _templateModel.EnsureNotSet(nameof(BulkEmailMessage.TemplateModel));

        var (snapshot, serializedSizeInBytes) = templateModel.SnapshotTemplateModel(nameof(templateModel));

        _templateModel = templateModel;
        _templateModelSnapshot = snapshot;
        _templateModelSizeInBytes = serializedSizeInBytes;

        return this;
    }
}
