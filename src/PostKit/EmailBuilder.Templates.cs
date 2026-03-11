using System.Text.RegularExpressions;
using PostKit.Common;

namespace PostKit;

partial class EmailBuilder
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
    private int? _templateId;
    private string? _templateAlias;
    private object? _templateModel;
    private bool? _inlineCss;

    /// <inheritdoc/>
    public IEmailBuilder WithTemplate(int templateId, object templateModel, bool? inlineCss = null)
    {
        _subject.EnsureNotSet(nameof(Email.Subject));
        _htmlBody.EnsureNotSet(nameof(Email.HtmlBody));
        _textBody.EnsureNotSet(nameof(Email.TextBody));
        _templateId.EnsureNotSet(nameof(Email.TemplateId));
        _templateAlias.EnsureNotSet(nameof(Email.TemplateAlias));
        _templateModel.EnsureNotSet(nameof(Email.TemplateModel));
        _inlineCss.EnsureNotSet(nameof(Email.InlineCss));

        if (templateId <= 0)
            throw new ArgumentException("The template ID must be greater than zero.", nameof(templateId));

        ArgumentNullException.ThrowIfNull(templateModel);

        _templateId = templateId;
        _templateModel = templateModel;
        _inlineCss = inlineCss;

        return this;
    }

    /// <inheritdoc/>
    public IEmailBuilder WithTemplate(string templateAlias, object templateModel, bool? inlineCss = null)
    {
        _subject.EnsureNotSet(nameof(Email.Subject));
        _htmlBody.EnsureNotSet(nameof(Email.HtmlBody));
        _textBody.EnsureNotSet(nameof(Email.TextBody));
        _templateId.EnsureNotSet(nameof(Email.TemplateId));
        _templateAlias.EnsureNotSet(nameof(Email.TemplateAlias));
        _templateModel.EnsureNotSet(nameof(Email.TemplateModel));
        _inlineCss.EnsureNotSet(nameof(Email.InlineCss));

        if (string.IsNullOrWhiteSpace(templateAlias))
            throw new ArgumentException("The template alias is required.", nameof(templateAlias));

        ArgumentNullException.ThrowIfNull(templateModel);

        if (templateAlias.Length > TemplateAliasMaxLength)
            throw new ArgumentException($"The template alias must not exceed {TemplateAliasMaxLength} characters.", nameof(templateAlias));
#if NET9_0_OR_GREATER
        if (!TemplateAliasRegex.IsMatch(templateAlias))
#else
        if (!TemplateAliasRegex().IsMatch(templateAlias))
#endif
            throw new ArgumentException("The template alias must start with a letter and may only contain letters, numbers, '-', '_', or '.' characters.", nameof(templateAlias));

        _templateAlias = templateAlias;
        _templateModel = templateModel;
        _inlineCss = inlineCss;

        return this;
    }
}
