using PostKit.Postmark.Common;

namespace PostKit.BulkEmails;

partial class BulkEmailBuilder
{
    private string? _htmlBody;
    private string? _textBody;

    /// <inheritdoc/>
    public IBulkEmailBuilder WithHtmlBody(string htmlBody)
    {
        ArgumentNullException.ThrowIfNull(htmlBody);

        _htmlBody.EnsureNotSet(nameof(BulkEmail.HtmlBody));
        _templateId.EnsureNotSet(nameof(BulkEmail.TemplateId));
        _templateAlias.EnsureNotSet(nameof(BulkEmail.TemplateAlias));

        _htmlBody = htmlBody;

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailBuilder WithTextBody(string textBody)
    {
        ArgumentNullException.ThrowIfNull(textBody);

        _textBody.EnsureNotSet(nameof(BulkEmail.TextBody));
        _templateId.EnsureNotSet(nameof(BulkEmail.TemplateId));
        _templateAlias.EnsureNotSet(nameof(BulkEmail.TemplateAlias));

        _textBody = textBody;

        return this;
    }
}
