using System.Text.RegularExpressions;
using PostKit.Postmark.Common;

namespace PostKit.BulkEmails;

partial class BulkEmailBuilder
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
    private bool? _inlineCss;

    /// <inheritdoc/>
    public IBulkEmailBuilder WithTemplate(int templateId, bool? inlineCss = null)
    {
        _subject.EnsureNotSet(nameof(BulkEmail.Subject));
        _htmlBody.EnsureNotSet(nameof(BulkEmail.HtmlBody));
        _textBody.EnsureNotSet(nameof(BulkEmail.TextBody));
        _templateId.EnsureNotSet(nameof(BulkEmail.TemplateId));
        _templateAlias.EnsureNotSet(nameof(BulkEmail.TemplateAlias));
        _inlineCss.EnsureNotSet(nameof(BulkEmail.InlineCss));

        if (templateId <= 0)
            throw new ArgumentException("The template ID must be greater than zero.", nameof(templateId));

        _templateId = templateId;
        _inlineCss = inlineCss;

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailBuilder WithTemplate(string templateAlias, bool? inlineCss = null)
    {
        _subject.EnsureNotSet(nameof(BulkEmail.Subject));
        _htmlBody.EnsureNotSet(nameof(BulkEmail.HtmlBody));
        _textBody.EnsureNotSet(nameof(BulkEmail.TextBody));
        _templateId.EnsureNotSet(nameof(BulkEmail.TemplateId));
        _templateAlias.EnsureNotSet(nameof(BulkEmail.TemplateAlias));
        _inlineCss.EnsureNotSet(nameof(BulkEmail.InlineCss));

        if (string.IsNullOrWhiteSpace(templateAlias))
            throw new ArgumentException("The template alias is required.", nameof(templateAlias));

        if (templateAlias.Length > TemplateAliasMaxLength)
            throw new ArgumentException($"The template alias must not exceed {TemplateAliasMaxLength} characters.", nameof(templateAlias));
#if NET9_0_OR_GREATER
        if (!TemplateAliasRegex.IsMatch(templateAlias))
#else
        if (!TemplateAliasRegex().IsMatch(templateAlias))
#endif
            throw new ArgumentException("The template alias must start with a letter and may only contain letters, numbers, '-', '_', or '.' characters.", nameof(templateAlias));

        _templateAlias = templateAlias;
        _inlineCss = inlineCss;

        return this;
    }
}
