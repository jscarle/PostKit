using PostKit.Postmark.Common;

namespace PostKit.BulkEmails;

partial class BulkEmailMessageBuilder
{
    private object? _templateModel;

    /// <inheritdoc/>
    public IBulkEmailMessageBuilder WithTemplateModel(object templateModel)
    {
        _templateModel.EnsureNotSet(nameof(BulkEmailMessage.TemplateModel));
        ArgumentNullException.ThrowIfNull(templateModel);

        _templateModel = templateModel;

        return this;
    }
}
