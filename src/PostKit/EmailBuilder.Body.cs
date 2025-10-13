using PostKit.Common;

namespace PostKit;

partial class EmailBuilder
{
    private string? _htmlBody;
    private string? _textBody;

    /// <inheritdoc />
    public IEmailBuilder WithHtmlBody(string html)
    {
        _templateId.EnsureNotSet(nameof(Email.TemplateId));
        _templateAlias.EnsureNotSet(nameof(Email.TemplateAlias));
        _templateModel.EnsureNotSet(nameof(Email.TemplateModel));
        _inlineCss.EnsureNotSet(nameof(Email.InlineCss));
        _htmlBody.EnsureNotSet(nameof(Email.HtmlBody));

        _htmlBody = html;

        return this;
    }

    /// <inheritdoc />
    public IEmailBuilder WithTextBody(string text)
    {
        _templateId.EnsureNotSet(nameof(Email.TemplateId));
        _templateAlias.EnsureNotSet(nameof(Email.TemplateAlias));
        _templateModel.EnsureNotSet(nameof(Email.TemplateModel));
        _inlineCss.EnsureNotSet(nameof(Email.InlineCss));
        _textBody.EnsureNotSet(nameof(Email.TextBody));

        _textBody = text;

        return this;
    }
}
