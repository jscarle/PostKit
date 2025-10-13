using System.Text.RegularExpressions;
using PostKit.Common;

namespace PostKit;

partial class EmailBuilder
{
    private int? _templateId;
    private string? _templateAlias;
    private object? _templateModel;
    private bool? _inlineCss;
    private const int TemplateAliasMaxLength = 64;

    /// <inheritdoc />
    public IEmailBuilder WithTemplate(int templateId, object templateModel, bool? inlineCss = null)
    {
        _htmlBody.EnsureNotSet(nameof(Email.HtmlBody));
        _textBody.EnsureNotSet(nameof(Email.TextBody));
        _templateId.EnsureNotSet(nameof(Email.TemplateId));
        _templateAlias.EnsureNotSet(nameof(Email.TemplateAlias));
        _templateModel.EnsureNotSet(nameof(Email.TemplateModel));
        _inlineCss.EnsureNotSet(nameof(Email.InlineCss));

        if (templateId <= 0)
            throw new ArgumentException("The template ID must be greater than zero.", nameof(templateId));

        if (templateModel is null)
            throw new ArgumentException("The template model is required.", nameof(templateModel));

        _templateId = templateId;
        _templateModel = templateModel;
        _inlineCss = inlineCss;

        return this;
    }

    /// <inheritdoc />
    public IEmailBuilder WithTemplate(string templateAlias, object templateModel, bool? inlineCss = null)
    {
        _htmlBody.EnsureNotSet(nameof(Email.HtmlBody));
        _textBody.EnsureNotSet(nameof(Email.TextBody));
        _templateId.EnsureNotSet(nameof(Email.TemplateId));
        _templateAlias.EnsureNotSet(nameof(Email.TemplateAlias));
        _templateModel.EnsureNotSet(nameof(Email.TemplateModel));
        _inlineCss.EnsureNotSet(nameof(Email.InlineCss));

        if (string.IsNullOrWhiteSpace(templateAlias))
            throw new ArgumentException("The template alias is required.", nameof(templateAlias));

        if (templateAlias.Length > TemplateAliasMaxLength)
            throw new ArgumentException(
                $"The template alias must not exceed {TemplateAliasMaxLength} characters.",
                nameof(templateAlias));

        if (!TemplateAliasRegex.IsMatch(templateAlias))
            throw new ArgumentException(
                "The template alias must start with a letter and may only contain letters, numbers, '-', '_', or '.' characters.",
                nameof(templateAlias));

        if (templateModel is null)
            throw new ArgumentException("The template model is required.", nameof(templateModel));

        _templateAlias = templateAlias;
        _templateModel = templateModel;
        _inlineCss = inlineCss;

        return this;
    }

    [GeneratedRegex(@"^[A-Za-z][A-Za-z0-9._-]*$")]
    private static partial Regex TemplateAliasRegex { get; }
}