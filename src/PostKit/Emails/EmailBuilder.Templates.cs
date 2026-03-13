using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using PostKit.Postmark.Common;

namespace PostKit.Emails;

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
    private JsonNode? _templateModelSnapshot;
    private int _templateModelSizeInBytes;
    private bool? _inlineCss;

    /// <inheritdoc/>
    public IEmailBuilder UsingTemplate(int templateId, bool? inlineCss = null)
    {
        _subject.EnsureNotSet(nameof(Email.Subject));
        _htmlBody.EnsureNotSet(nameof(Email.HtmlBody));
        _textBody.EnsureNotSet(nameof(Email.TextBody));
        _templateId.EnsureNotSet(nameof(Email.TemplateId));
        _templateAlias.EnsureNotSet(nameof(Email.TemplateAlias));
        _inlineCss.EnsureNotSet(nameof(Email.InlineCss));

        if (templateId <= 0)
            throw new ArgumentException("The template ID must be greater than zero.", nameof(templateId));

        _templateId = templateId;
        _inlineCss = inlineCss;

        return this;
    }

    /// <inheritdoc/>
    public IEmailBuilder UsingTemplate(string templateAlias, bool? inlineCss = null)
    {
        _subject.EnsureNotSet(nameof(Email.Subject));
        _htmlBody.EnsureNotSet(nameof(Email.HtmlBody));
        _textBody.EnsureNotSet(nameof(Email.TextBody));
        _templateId.EnsureNotSet(nameof(Email.TemplateId));
        _templateAlias.EnsureNotSet(nameof(Email.TemplateAlias));
        _inlineCss.EnsureNotSet(nameof(Email.InlineCss));

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

    /// <inheritdoc/>
    public IEmailBuilder WithTemplateModel(object templateModel)
    {
        _templateModel.EnsureNotSet(nameof(Email.TemplateModel));

        var (snapshot, serializedSizeInBytes) = templateModel.SnapshotTemplateModel(nameof(templateModel));

        _templateModel = templateModel;
        _templateModelSnapshot = snapshot;
        _templateModelSizeInBytes = serializedSizeInBytes;

        return this;
    }
}
