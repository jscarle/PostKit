using System.Text.Json;
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
        return SetTemplateModel(templateModel);
    }

    /// <summary>Sets the recipient-specific template model using explicit serializer options for this call.</summary>
    /// <param name="templateModel">The template model data.</param>
    /// <param name="serializerOptions">The serializer options to use for this template model.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public IBulkEmailMessageBuilder WithTemplateModel(object templateModel, JsonSerializerOptions serializerOptions)
    {
        ArgumentNullException.ThrowIfNull(serializerOptions);
        return SetTemplateModel(templateModel, serializerOptions);
    }

    private BulkEmailMessageBuilder SetTemplateModel(object templateModel, JsonSerializerOptions? serializerOptions = null)
    {
        _templateModel.EnsureNotSet(nameof(BulkEmailMessage.TemplateModel));

        var (snapshot, serializedSizeInBytes) = templateModel.SnapshotTemplateModel(nameof(templateModel), serializerOptions);

        _templateModel = templateModel;
        _templateModelSnapshot = snapshot;
        _templateModelSizeInBytes = serializedSizeInBytes;

        return this;
    }
}
